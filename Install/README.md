# Пакеты deb и rpm для Service.Registry.Host

Пакет `bsa-serviceregistry` ставит реестр сервисов в `/opt/bsa-serviceregistry` и запускает его как службу systemd
`bsa-serviceregistry` от системного пользователя `bsaserviceregistry`.

## Сборка

Нужны `dpkg-deb` и .NET SDK целевой версии проекта (сейчас `net6.0`). Проще всего собирать в контейнере SDK
(образ на Debian, `dpkg-deb` в нём есть), из PowerShell:

```powershell
cd D:\Work\ServiceRegistry
docker run --rm -v "${PWD}:/src" -w /src mcr.microsoft.com/dotnet/sdk:6.0 bash Install/build-deb.sh
```

Или в Linux / WSL (Ubuntu, не служебный дистрибутив `docker-desktop`):

```bash
cd /mnt/d/Work/ServiceRegistry
./Install/build-deb.sh                          # framework-dependent, linux-x64
SELF_CONTAINED=true ./Install/build-deb.sh      # с рантаймом .NET внутри пакета
RID=linux-arm64 ./Install/build-deb.sh          # для arm64
```

Готовый пакет: `Install/out/bsa-serviceregistry_<версия>_<arch>.deb`. Версия берётся из `<Version>` в
`Service.Registry.Host.csproj` (или из переменной `VERSION`).

Если публикация уже сделана (например, в Visual Studio под `linux-x64`), её можно передать без повторного `dotnet publish`:

```bash
PUBLISH_DIR=/mnt/d/Work/ServiceRegistry/Service.Registry.Host/bin/Release/net6.0/linux-x64/publish ./Install/build-deb.sh
```

## Установка

```bash
sudo apt install ./bsa-serviceregistry_6.0.47_amd64.deb   # подтянет aspnetcore-runtime-6.0
systemctl status bsa-serviceregistry
journalctl -u bsa-serviceregistry -f
curl http://localhost:7777/api/service/all
```

HTTP API: `http://<сервер>:7777/api/service/...` — `Get?name=`, `all[?name=]`, `Register`, `Remove`, `Test?name=`.

## Где что лежит

| Путь | Что |
|---|---|
| `/opt/bsa-serviceregistry` | приложение; `appsettings.json` здесь — ссылка на `/etc/bsa-serviceregistry` |
| `/etc/bsa-serviceregistry/appsettings.json` | адрес и порт HTTP (`ServiceRegistry:Host`, по умолчанию `http://+:7777`), адрес самого реестра для `Test` (`ServiceRegistry:address`) и список сервисов `Clients` |
| журнал systemd | логи (`journalctl -u bsa-serviceregistry`) |

`Clients` — сервисы, известные реестру при запуске: `id` — имя (обычно имя интерфейса клиента, например
`IFolderStorageRestService`), `host` — адрес REST-сервиса, `address` и `port` — TCP-сервиса. По умолчанию там
FileStorage на этом же сервере (`localhost:11580` и `127.0.0.1:11581`) — замените на реальные адреса.
Сервисы, зарегистрированные через `Register`, хранятся в памяти и после перезапуска службы теряются
(сервисы регистрируются заново при своём запуске).

Файл в `/etc/bsa-serviceregistry` — conffile: при обновлении пакета dpkg сохраняет локальные изменения
(и спрашивает, если в новой версии файл тоже изменился). После правки конфигурации:
`sudo systemctl restart bsa-serviceregistry`.

## Обновление и удаление

- `apt install ./новый.deb` — служба останавливается, пакет обновляется, служба запускается.
- `apt remove bsa-serviceregistry` — удаляет приложение и службу; конфигурация остаётся.
- `apt purge bsa-serviceregistry` — дополнительно удаляет конфигурацию и пользователя `bsaserviceregistry`.

## Замечания

- Приложение ничего не пишет на диск: `/opt/bsa-serviceregistry` принадлежит root, логи идут в журнал systemd,
  у пользователя службы нет домашней папки. В юните включены `ProtectSystem=full`, `ProtectHome`, `PrivateTmp`.
- Подробность логов — `Logging:LogLevel` в `appsettings.json`. Строка на каждый HTTP-запрос пишется на уровне Debug:
  чтобы её включить, задайте `"Service.Registry.Host": "Debug"` и перезапустите службу.
- API не проверяет доступ и слушает все интерфейсы: любой, кто достучится до порта 7777, может зарегистрировать или
  удалить сервис. Доступ к порту стоит ограничить межсетевым экраном; чтобы реестр был доступен только локально,
  укажите `"Host": "http://localhost:7777"`.
- Зависимость пакета — `aspnetcore-runtime-<версия>` по `<TargetFramework>` проекта (для `net6.0` — `aspnetcore-runtime-6.0`).
  .NET 6 больше не поддерживается, и в свежих дистрибутивах этого пакета может не быть (в репозитории Microsoft
  `packages.microsoft.com` он есть). Для систем без него — сборка с `SELF_CONTAINED=true` (рантайм внутри пакета).
  После перевода проекта на новую версию .NET зависимость поменяется сама.

## Пакет rpm (RHEL, Rocky, Alma, Fedora, РЕД ОС)

Файлы: `build-rpm.sh` и `rpm/bsa-serviceregistry.spec`. Раскладка, служба, пользователь и конфигурация те же, что у deb
(конфиг — `%config(noreplace)`, сохраняется при обновлении; юнит — `/usr/lib/systemd/system`).

```powershell
cd D:\Work\ServiceRegistry
docker run --rm -v "${PWD}:/src" -w /src mcr.microsoft.com/dotnet/sdk:6.0 bash Install/build-rpm.sh
```

Скрипт сам ставит `rpmbuild` в контейнер (пакет `rpm`). Результат: `Install\out\bsa-serviceregistry-<версия>-1.x86_64.rpm`.
Те же переменные: `SELF_CONTAINED=true`, `RID=linux-arm64`, `VERSION`, `PUBLISH_DIR`.

```bash
sudo dnf install ./bsa-serviceregistry-6.0.47-1.x86_64.rpm   # подтянет aspnetcore-runtime-6.0
systemctl status bsa-serviceregistry
```

- `dnf remove bsa-serviceregistry` — служба останавливается и удаляется; изменённый конфиг сохраняется как `*.rpmsave`.
- `aspnetcore-runtime-6.0` был в AppStream RHEL 8/9, но снят с поддержки; надёжнее — `SELF_CONTAINED=true`.
