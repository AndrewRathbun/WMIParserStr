# WMIParserStr

This repo is an updated fork of the original project, found [here](https://github.com/ignacioj/WMIParserStr). All credit due to the original author of this tool. The only thing this fork has done over the original repo (so far) is an updated README, providing a compiled, signed binary, and providing the binary in the Releases section for ease of access.

## WMI OBJECTS.DATA parser

Very fast. It can extract Consumers and EventFilters deleted or without a binding. These orphans are marked as TRUE in the last column of the report. False for the bindings and those Consumers and the EventConsumer that are binded.

There will be false positives due to the search method.

## Parameters

Below is a current list of parameters, whether they're mandatory or optional, and a description of each switch.
 
`-input` - Mandatory - Input file (OBJECTS.DATA)

`-output` - Optional - Output directory for analysis results. Tab delimited file

`-strings` - Optional - Output directory to save the strings (not Unicode) of OBJECTS.DATA

## Example Commands

`WMIParserStr.exe -input C:\temp\test\OBJECTS.DATA -output C:\temp\test\TSV -strings C:\temp\test\Strings`

## Console output:

When running this tool with or without any of the optional switches, the tool will print output similar to the example below to the console.

```
Total Bindings: 22

[Binding]-[CommandLineEventConsumer]-[ConsumerA]-[Test]-[False]

[Binding]-[CommandLineEventConsumer]-[ConsumerA]-[Test]-[False]

[Binding]-[CommandLineEventConsumer]-[ConsumerA]-[Test]-[False]

[Binding]-[CommandLineEventConsumer]-[BotConsumer23]-[BotFilter82]-[False]

[Binding]-[CommandLineEventConsumer]-[ConsumerTest]-[Test]-[False]

[Binding]-[CommandLineEventConsumer]-[ConsumerA]-[Test]-[False]

[Binding]-[CommandLineEventConsumer]-[ConsumerA]-[Test]-[False]

[Binding]-[NTEventLogEventConsumer]-[SCM Event Log Consumer]-[SCM Event Log Filter]-[False]

...........

Total Consumers: 120

[CommandLineEventConsumer]-[InfectDrive]-[powershell.exe -NoP -C [Text.Encoding]::ASCII.GetString([Convert]::FromBase64String('WDVPIVAlQEFQWzRcUFpYNTQoUF4pN0NDKTd9JEVJQ0FSLVNUQU5EQVJELUFOVElWSVJVUy1URVNULUZJTEUhJEgrSCo=')) | Out-File %DriveName%\eicar.txt]-[]-[True]

[ActiveScriptEventConsumer]-[CleanupFileNames2]-[C:\fso\LaunchPowerShell.vbs]-[VBScript]-[True]

...........

Total EventFilters: 22

[__EventFilter]-[uint8]-[EventAccessstringEventNamespacestringNamestringQuery]-[CreatorSID]-[True]

[__EventFilter]-[uint8]-[EventAccessstringEventNamespacestringNamestringQuery]-[CreatorSID]-[True]

[__EventFilter]-[Test]-[SELECT * FROM __InstanceCreationEvent WITHIN 5 WHERE TargetInstance ISA 'CIM_DataFile'     AND TargetInstance.Drive = 'c:'     AND TargetInstance.Path = '\\test\\'     AND TargetInstance.Extension = 'txt']-[root\cimv2]-[False]

[__EventFilter]-[VolumeDetection]-[SELECT * FROM Win32_VolumeChangeEvent WHERE EventType=2]-[root\cimv2]-[False]

[__EventFilter]-[Backdoor Registry Filter]-[SELECT * FROM RegistryValueChangeEvent WHERE Hive='HKEY_LOCAL_MACHINE' AND KeyPath='SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\' AND ValueName = 'Registry Backdoor']-[root/cimv2]-[True]
```

## Tab delimited file:

When using the `-o` switch, a TSV file will be created that will look similar to below:

```
Headers:              Type||       Name     ||       Content     ||              Other               ||Orphan



Bindings:          Binding||Type of Consumer|| Consumer name     ||        EventFilter name          ||FALSE

Consumers:            Type||       name     ||CommandLineTemplate||[ExecutablePath][VBScript/JSCript]||False/True 

EventFilter: __EventFilter||       name     ||    Condition      ||           [root\cimv2][...]      ||False/True
```
