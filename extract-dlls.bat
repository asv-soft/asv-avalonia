@echo off
setlocal enabledelayedexpansion

:: Path to the build directory (default is the current directory)
set BuildDir=%1
if "%BuildDir%"=="" (
    echo Usage: extract-dlls.bat <path_to_build_directory> [output_directory]
    exit /b 1
)

:: Check if the build directory exists
if not exist "%BuildDir%" (
    echo Error: Directory %BuildDir% not found.
    exit /b 1
)

:: Path to the directory where the output file will be saved (if not provided, save in the current directory)
set OutputDir=%2
if "%OutputDir%"=="" (
    set OutputDir=%CD%
)

:: Create the path for the output file
set OutputFile=%OutputDir%\IncludedPackages.txt

:: If the file already exists, overwrite it
if exist "%OutputFile%" (
    echo File %OutputFile% already exists, overwriting...
    del "%OutputFile%"
)

:: Loop through all .dll files in the build directory
echo Extracting DLLs from %BuildDir%

for /r "%BuildDir%" %%f in (*.dll) do (
    set FileName=%%~nxf
    set BaseName=%%~nF
    :: Exclude files with the same name but with a .exe extension
    if not exist "%BuildDir%\!BaseName!.exe" (
        echo !FileName! >> "%OutputFile%"
    )
)

echo Extraction complete. List of DLLs saved to %OutputFile%.

endlocal
