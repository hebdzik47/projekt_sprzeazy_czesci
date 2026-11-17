$ErrorActionPreference = 'Stop'
$rule = Get-NetFirewallRule -Name 'AutoPartsSales-LAN' -ErrorAction SilentlyContinue
if ($rule) { Disable-NetFirewallRule -Name 'AutoPartsSales-LAN' }
