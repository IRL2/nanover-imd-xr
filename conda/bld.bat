REM @echo on

mkdir %SCRIPTS%
robocopy %RECIPE_DIR%\artifacts\StandaloneWindows64 %SCRIPTS%\NanoVer-iMD-XR /e
REM Make NanoverImd available in the Path while keeping it in
REM its directory.
set local_script=%%CONDA_PREFIX%%\Scripts%
echo "%local_script%\NanoVer-iMD-XR\NanoVer iMD.exe" > %SCRIPTS%\NanoVer-iMD-XR.bat
exit 0
