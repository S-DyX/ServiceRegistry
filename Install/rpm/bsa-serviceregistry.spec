# RPM spec of Service.Registry.Host as a systemd service.
# Built by Install/build-rpm.sh: the application is published beforehand and staged into the stagedir define;
# the spec only packages the staged tree. Defines passed by the script: pkgversion, pkgrequires, stagedir.

# prebuilt .NET binaries: no debug package, no stripping, no build-id links, no automatic dependency scan
%global debug_package %{nil}
%global __strip /bin/true
%global __os_install_post %{nil}
%define _build_id_links none

Name:           bsa-serviceregistry
Version:        %{pkgversion}
Release:        1%{?dist}
Summary:        BSA Service Registry host - addresses of REST and TCP services
License:        Apache-2.0
URL:            https://bsasearch.org/serviceregistry.html
AutoReqProv:    no
Requires:       %{pkgrequires}
Requires(pre):  shadow-utils
Requires(post): systemd
Requires(preun): systemd
Requires(postun): systemd

%description
HTTP API (ASP.NET Core) that returns addresses of REST and TCP services by
name: services from the configuration and services registered at runtime,
with round-robin choice of the least used address.
Runs as the systemd service bsa-serviceregistry.

%prep

%build

%install
mkdir -p %{buildroot}
cp -a %{stagedir}/. %{buildroot}/

%pre
getent group bsaserviceregistry >/dev/null || groupadd -r bsaserviceregistry
getent passwd bsaserviceregistry >/dev/null || \
	useradd -r -M -g bsaserviceregistry -d / -s /sbin/nologin -c "BSA Service Registry" bsaserviceregistry
exit 0

%post
# $1: 1 - install, 2 - upgrade (the service is restarted in %%postun of the old package)
if [ -d /run/systemd/system ]; then
	systemctl daemon-reload >/dev/null 2>&1 || :
	if [ "$1" -eq 1 ]; then
		systemctl enable bsa-serviceregistry.service >/dev/null 2>&1 || :
		systemctl start bsa-serviceregistry.service || echo "bsa-serviceregistry: service did not start, see: journalctl -u bsa-serviceregistry"
	fi
fi
exit 0

%preun
# $1: 0 - removal, 1 - upgrade
if [ "$1" -eq 0 ] && [ -d /run/systemd/system ]; then
	systemctl stop bsa-serviceregistry.service || :
	systemctl disable bsa-serviceregistry.service >/dev/null 2>&1 || :
fi
exit 0

%postun
if [ -d /run/systemd/system ]; then
	systemctl daemon-reload >/dev/null 2>&1 || :
	if [ "$1" -ge 1 ]; then
		systemctl try-restart bsa-serviceregistry.service || :
	fi
fi
exit 0

%files
# the application does not write to disk: owned by root, logs go to the systemd journal
/opt/bsa-serviceregistry
%dir %attr(0750,root,bsaserviceregistry) /etc/bsa-serviceregistry
%config(noreplace) %attr(0640,root,bsaserviceregistry) /etc/bsa-serviceregistry/*
/usr/lib/systemd/system/bsa-serviceregistry.service
