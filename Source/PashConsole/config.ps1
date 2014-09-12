# DEPRECATED

#This file is currently not in use. It would be nice, if we could use it, but it slows down the startup by about 200ms.
#While this is okay for the Pash shell, it's really bad for the tests, as they will take way to long.
#Unless a better solution is found, we won't use this file anymore, but therefore use InitialSessionState.CreateDefault()



function Prompt {
    'PASH ' + (Get-Location) + '> '
}

# TODO: Use `Set-Variable` to make this readonly,
# e.g.
#    Set-Variable -Name Host -Value (Get-Host) -Options ReadOnly
$Host = Get-Host

#function prompt { 'PASH ' + $(Get-Location) + $(if ($nestedpromptlevel -ge 1) { '>>' }) + '> ' }
#. HelpPagingFunction.Library.ps1
      
#function mkdir { param([string[]]$paths); New-Item -type directory -path $paths }
#function md { param([string[]]$paths); New-Item -type directory -path $paths }
#. TabExpansionFunction.Library.ps1
#function Clear-Host { $spaceType = [System.Management.Automation.Host.BufferCell]; $space = [System.Activator]::CreateInstance($spaceType); $space.Character = ' '; $space.ForegroundColor = $host.ui.rawui.ForegroundColor; $space.BackgroundColor = $host.ui.rawui.BackgroundColor; $rectType = [System.Management.Automation.Host.Rectangle]; $rect = [System.Activator]::CreateInstance($rectType); $rect.Top = $rect.Bottom = $rect.Right = $rect.Left = -1; $Host.UI.RawUI.SetBufferContents($rect, $space); $coordType = [System.Management.Automation.Host.Coordinates]; $origin = [System.Activator]::CreateInstance($coordType); $Host.UI.RawUI.CursorPosition = $origin;  }
#function more { param([string[]]$paths);  if(($paths -ne $null) -and ($paths.length -ne 0))  {    foreach ($local:file in $paths)    {        Get-Content $local:file | Out-Host -p    }  }  else { $input | Out-Host -p } " / -->

function A: { Set-Location A: }
function B: { Set-Location B: }
function C: { Set-Location C: }
function D: { Set-Location D: }
function E: { Set-Location E: }
function F: { Set-Location F: }
function G: { Set-Location G: }
function H: { Set-Location H: }
function I: { Set-Location I: }
function J: { Set-Location J: }
function K: { Set-Location K: }
function L: { Set-Location L: }
function M: { Set-Location M: }
function N: { Set-Location N: }
function O: { Set-Location O: }
function P: { Set-Location P: }
function Q: { Set-Location Q: }
function R: { Set-Location R: }
function S: { Set-Location S: }
function T: { Set-Location T: }
function U: { Set-Location U: }
function V: { Set-Location V: }
function W: { Set-Location W: }
function X: { Set-Location X: }
function Y: { Set-Location Y: }
function Z: { Set-Location Z: }

Set-Alias gcm Get-Command
Set-Alias pwd Get-Location
Set-Alias dir Get-ChildItem
Set-Alias ls Get-ChildItem
Set-Alias cd Set-Location
Set-Alias chdir Set-Location
Set-Alias set Set-Variable

Set-Alias echo Write-Output
Set-Alias sort Sort-Object
Set-Alias man help

#      Set-Alias type Get-Content
#
#      Set-Alias ac Add-Content
#      Set-Alias asnp Add-PSSnapin
#      Set-Alias cat Get-Content
#      Set-Alias clc Clear-Content
#      Set-Alias clear Clear-Host
#      Set-Alias cls Clear-Host
#      Set-Alias cli Clear-Item
#      Set-Alias clp Clear-ItemProperty
#      Set-Alias clv Clear-Variable
#      Set-Alias copy Copy-Item
#      Set-Alias cp Copy-Item
#      Set-Alias cpi Copy-Item
#      Set-Alias cpp Copy-ItemProperty
#      Set-Alias cvpa Convert-Path
#      Set-Alias diff Compare-Object
#      Set-Alias epal Export-Alias
#      Set-Alias epcsv Export-Csv
#      Set-Alias fc Format-Custom
#      Set-Alias fl Format-List
Set-Alias foreach ForEach-Object
Set-Alias % ForEach-Object
#      Set-Alias ft Format-Table
#      Set-Alias fw Format-Wide
#      Set-Alias gal Get-Alias
#      Set-Alias gc Get-Content
#      Set-Alias gci Get-ChildItem
#      Set-Alias gdr Get-PSDrive
#      Set-Alias ghy Get-History
#      Set-Alias gi Get-Item
#      Set-Alias gl Get-Location
#      Set-Alias gm Get-Member
#      Set-Alias gp Get-ItemProperty
#      Set-Alias gps Get-Process
#      Set-Alias group Group-Object
#      Set-Alias gsv Get-Service
#      Set-Alias gsnp Get-PSSnapin
#      Set-Alias gu Get-Unique
#      Set-Alias gv Get-Variable
#      Set-Alias gwmi Get-WmiObject
#      Set-Alias h Get-History
#      Set-Alias history Get-History
#      Set-Alias iex Invoke-Expression
#      Set-Alias ihy Invoke-History
#      Set-Alias r Invoke-History
#      Set-Alias ii Invoke-Item
#      Set-Alias ipal Import-Alias
#      Set-Alias ipcsv Import-Csv
#      Set-Alias kill Stop-Process
#      Set-Alias lp Out-Printer
#      Set-Alias mount New-PSDrive
#      
#      Set-Alias move Move-Item
#      Set-Alias mv Move-Item
#      Set-Alias mi Move-Item
#      
#      Set-Alias mp Move-ItemProperty
#      Set-Alias nal New-Alias
#      Set-Alias ndr New-PSDrive
#      Set-Alias ni New-Item
#      Set-Alias nv New-Variable
#      Set-Alias oh Out-Host
#
#      Set-Alias ren Rename-Item
#      Set-Alias rdr Remove-PSDrive
#      Set-Alias del Remove-Item
#      Set-Alias erase Remove-Item
#      Set-Alias rd Remove-Item
#      Set-Alias ri Remove-Item
#      Set-Alias rm Remove-Item
#      Set-Alias rmdir Remove-Item
#      Set-Alias rni Rename-Item
#      Set-Alias rnp Rename-ItemProperty
#      Set-Alias rp Remove-ItemProperty
#      Set-Alias rsnp Remove-PSSnapin
#      Set-Alias rv Remove-Variable
#
#      Set-Alias rvpa Resolve-Path
#      Set-Alias sal Set-Alias
#      Set-Alias sasv Start-Service
#      Set-Alias sc Set-Content
#      Set-Alias select Select-Object
#      Set-Alias si Set-Item
#      Set-Alias sl Set-Location
#      Set-Alias sleep Start-Sleep
#      Set-Alias sp Set-ItemProperty
#      Set-Alias spps Stop-Process
#      Set-Alias spsv Stop-Service
#      Set-Alias sv Set-Variable
#      Set-Alias tee Tee-Object
#      Set-Alias ? Where-Object
#      Set-Alias where Where-Object
#      Set-Alias write Write-Output
#
#      Set-Alias popd Pop-Location
#      Set-Alias ps Get-Process
#      Set-Alias pushd Push-Location
#
