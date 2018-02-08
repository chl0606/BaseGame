
@ECHO OFF

:: 사용법 ExportConverToTXT.bat [excel file이름] [excel sheet이름] [table이름] [true|false] nopause
:: 4번째 인자값이 true 면 파일이름을 table이름과 같게 변경한다.


REM 컨버팅 툴 경로
set CONVERTER_TOOL=%cd%\..\Tools\TableConverter.exe


REM 변환할 엑셀파일이 있는 경로
set TABLE_FILE=%cd%\..\Data\Excel\%~1


REM 변환된 테이블이 저장될 경로
set TEMP_OUTPUT_DIR=%cd%\.table
set TABLE_DIR=%cd%\..\Assets\Resources\Tables\

set SHEET_NAME=%2
set TABLE_NAME=%3
set RENAME_TXT_FILE=%~4



REM 데이타포멧은 XML, TXT 를 선택할 수 있다.
REM XML은 당분간 사용하지 마세요. 업데이트가 필요합니다.
set DATA_FORMAT=TXT

REM TODO
REM SVN에서 Updat가 필요한지 확인
REM diff로 파일 비교

if "%~3"=="" (
	goto end_convert
)

:: 임시 저장 폴더 생성
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

:: 임시 저장 폴더 삭제
del %TEMP_OUTPUT_DIR%\*.* /Q/S > NUL
rmdir %TEMP_OUTPUT_DIR%

:end_convert

if not "%~5"=="nopause" (
	pause
)