@ECHO OFF

setlocal ENABLEEXTENSIONS

TITLE Convert To XML

echo Converting Tables.....Wait..

:: ExcelConvertToTXT [execl����] [excel sheet�̸�] [table �̸�] [���ϸ���true|false] nopuase
::call ExcelConvertToTXT.bat "ConfigTable.xlsx" "GeneralSetting, MiniGameSetting" "GeneralSettingData, MiniGameSettingData" false nopause
call ExcelConvertToTXT.bat "ConfigTable.xlsx" "GeneralSetting" "GeneralSettingData" false nopause

endlocal


pause