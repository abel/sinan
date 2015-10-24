xcopy E:\Work\Sinan2\47BabyService\bin\Release\*.* E:\Work\47Baby\server\布署\47babyService /Q /R /Y
xcopy E:\Work\Sinan2\47BabyService\bin\Release\Plugins\*.* E:\Work\47Baby\server\布署\47babyService\Plugins /Q /R /Y
rd E:\Work\47Baby\server\布署\47babyService\Config /s /q
move E:\Work\Sinan2\FrontServer\bin\Release\Config E:\Work\47Baby\server\布署\47babyService
xcopy E:\Work\47Baby\server\布署\47babyService\Config\*.txt E:\Work\47Baby\server\布署\47babyService\Temp  /Q /R /Y
xcopy E:\Work\Sinan2\RepairModule\bin\Release\Sinan.RepairModule.* E:\Work\47Baby\server\布署\47babyService\Temp  /Q /R /Y
xcopy E:\Work\Sinan2\ManageModule\bin\Release\Sinan.ManageModule.* E:\Work\47Baby\server\布署\47babyService\Temp  /Q /R /Y
