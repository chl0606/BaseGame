
@ECHO OFF

:: ���� ExportConverToTXT.bat [excel file�̸�] [excel sheet�̸�] [table�̸�] [true|false] nopause
:: 4��° ���ڰ��� true �� �����̸��� table�̸��� ���� �����Ѵ�.


REM ������ �� ���
set CONVERTER_TOOL=%cd%\..\Tools\TableConverter.exe


REM ��ȯ�� ���������� �ִ� ���
set TABLE_FILE=%cd%\..\Data\Excel\%~1


REM ��ȯ�� ���̺��� ����� ���
set TEMP_OUTPUT_DIR=%cd%\.table
set TABLE_DIR=%cd%\..\Assets\Resources\Tables\

set SHEET_NAME=%2
set TABLE_NAME=%3
set RENAME_TXT_FILE=%~4



REM ����Ÿ������ XML, TXT �� ������ �� �ִ�.
REM XML�� ��а� ������� ������. ������Ʈ�� �ʿ��մϴ�.
set DATA_FORMAT=TXT

REM TODO
REM SVN���� Updat�� �ʿ����� Ȯ��
REM diff�� ���� ��

if "%~3"=="" (
	goto end_convert
)

:: �ӽ� ���� ���� ����
if EXIST %TEMP_OUTPUT_DIR% (
	del %TEMP_OUTPUT_DIR%\*.* /Q/S > NUL
) else (
	mkdir %TEMP_OUTPUT_DIR%
)

set MANIFEST_CS=%cd%\..\Assets\Scripts\Tables\DataManifest.cs
set TMP_MANIFEST_CS=%TEMP_OUTPUT_DIR%\DataManifest.cs


echo =======================================================

%CONVERTER_TOOL% %TABLE_FILE% %TEMP_OUTPUT_DIR%\ %SHEET_NAME% %TABLE_NAME% %DATA_FORMAT% %TABLE_DIR%\DataManifest.xml %TMP_MANIFEST_CS%
if "%RENAME_TXT_FILE%"=="true" (
	ren %TEMP_OUTPUT_DIR%\%SHEET_NAME%.txt %TABLE_NAME%.txt
)
echo =======================================================

%cd%\..\Tools\diff\diff.exe %MANIFEST_CS% %TMP_MANIFEST_CS% > NUL
if not %errorlevel%==0 (
	echo CHANGED: %MANIFEST_CS%. Need to copy the file
	copy /Y %TMP_MANIFEST_CS% %MANIFEST_CS%
)
del %TMP_MANIFEST_CS% /Q/S > NUL

copy %TEMP_OUTPUT_DIR%\*	%TABLE_DIR%

:: �ӽ� ���� ���� ����
del %TEMP_OUTPUT_DIR%\*.* /Q/S > NUL
rmdir %TEMP_OUTPUT_DIR%

:end_convert

if not "%~5"=="nopause" (
	pause
)