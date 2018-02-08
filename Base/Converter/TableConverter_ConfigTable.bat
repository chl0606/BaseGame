@ECHO OFF

setlocal ENABLEEXTENSIONS

TITLE Convert To XML

echo Converting Tables.....Wait..

:: ExcelConvertToTXT [execl파일] [excel sheet이름] [table 이름] [파일명변경true|false] nopuase
::call ExcelConvertToTXT.bat "ConfigTable.xlsx" "GeneralSetting, MiniGameSetting" "GeneralSettingData, MiniGameSettingData" false nopause
call ExcelConvertToTXT.bat "ConfigTable.xlsx" "GeneralSetting" "GeneralSettingData" false nopause

endlocal


pause