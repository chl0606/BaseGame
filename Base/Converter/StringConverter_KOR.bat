
@ECHO OFF
TITLE Convert To XML

PUSHD %~DP0
REM 현재 배치파일의 디렉토리
set CURRENT_DIR=%CD%
set CONVERTER_DIR=%CURRENT_DIR%\../\Tools\

CD ..

REM 엑셀파일이 있는 디렉토리를 셋팅해야 한다.
set EXCEL_DIR=%CURRENT_DIR%\../\Data\Excel\
set EXCEL_NAME=StringTable.xlsx

REM 데이타포멧은 XML, TXT 를 선택할 수 있다.
REM XML은 당분간 사용하지 마세요. 업데이트가 필요합니다.
set DATA_FORMAT=TXT
REM KOR-한국, JPN-일본, INT-영어국가 (나라가 추가되면 툴 업데이트가 필요합니다.)
set EXPORT_NATION=KOR

REM TODO
REM SVN에서 Updat가 필요한지 확인
REM diff로 파일 비교


echo Converting Tables.....Wait..

REM GameUI Localize 테이블
set GAMEUI_DIR=%CURRENT_DIR%\../\Assets\Resources\Localize\
set GAMEUI_TABLE=GameUI
set GAMEUI_DATA=GameUIData
REM 0-ID하나에 스트링하나, 1-ID하나에 스트링 N개
set GAMEUI_STRINGTYPE=0
%CONVERTER_DIR%\LocalizeStringConvert.exe %EXCEL_DIR%%EXCEL_NAME% %GAMEUI_DIR%%GAMEUI_TABLE% %GAMEUI_TABLE% %GAMEUI_DATA% %DATA_FORMAT% %GAMEUI_STRINGTYPE% %EXPORT_NATION%
echo [ %GAMEUI_TABLE% ] Completed


REM Item_Ingrediant Localize 테이블
set ITEMINGREDIANT_DIR=%CURRENT_DIR%\../\Assets\Resources\Localize\
set ITEMINGREDIANT_TABLE=ItemString
set ITEMINGREDIANT_DATA=ItemStringData
REM 0-ID하나에 스트링하나, 1-ID하나에 스트링 N개
set ITEMINGREDIANT_STRINGTYPE=1
%CONVERTER_DIR%\LocalizeStringConvert.exe %EXCEL_DIR%%EXCEL_NAME% %ITEMINGREDIANT_DIR%%ITEMINGREDIANT_TABLE% %ITEMINGREDIANT_TABLE% %ITEMINGREDIANT_DATA% %DATA_FORMAT% %ITEMINGREDIANT_STRINGTYPE% %EXPORT_NATION%
echo [ %ITEMINGREDIANT_TABLE% ] Completed

pause