@echo off 
setlocal EnableDelayedExpansion

set extraArgs=%1

set outBasePath=%~dp0%out\
set bmd2gltfBasePath=%~dp0%bmd2gltf\
set szstoolsBasePath=%~dp0%szstools\

:: TODO: Take this as an arg.
set romPath=roms\pkmn2.gcm
set hierarchyPath=%romPath%_dir\


:: Extracts file hierarchy from the given ROM
echo Checking for previously extracted file hierarchy.
if not exist "%hierarchyPath%" (
  echo|set /p="Extracting file hierarchy... "
  "%szstoolsBasePath%gcmdump.exe" "%romPath%"
  echo OK!
  echo.
)


:: Navigates to hierarchy directory
pushd "%hierarchyPath%"

:: Iterates through all directories in the hierarchy
echo Checking for previously extracted archives.
set hierarchyListCmd="dir /b /s /ad *.* | sort"
for /f "tokens=*" %%d in ('%hierarchyListCmd%') do (
  cd "%%d"

  :: Extracts archives in the current directory
  for %%i in (*.szs) do (
    if not exist "%%i_dir" if not exist "%%i 0.rarc" (
      echo Extracting contents from %%d\%%i...
      "%szstoolsBasePath%yaz0dec.exe" "%%i"
      echo OK!
      echo.
    )
  )
  for %%i in (*.rarc) do (
    if not exist "%%i_dir" (
      echo Extracting contents from %%d\%%i...
      "%szstoolsBasePath%rarcdump.exe" "%%i"
      echo OK!
      echo.
    )
  )

  :: Gets model/animations
  set modelName=
  set modelFile=
  set animFiles=
  
  :: Gets from separate model/animation szs (e.g. Enemies)
  for /d %%m in (".\model.szs*.rarc_dir") do (
    pushd "%%m"
    for %%b in (".\model\*.bmd") do (
      set modelName=%%~nd
    )
    popd
  )

  :: Gets from model/animations in same szs (e.g. user\Kando)
  for /d %%r in (".\arc.szs*.rarc_dir") do (
    pushd "%%r"
    for %%b in (".\arc\*.bmd") do (
      set modelName=%%~nd
    )
    popd
  )

  :: Merges models + animations w/ automatic inputs
  if defined modelName (
    set modelBasePath=%outBasePath%!modelName!

    >nul 2>nul dir /a-d "!modelBasePath!\*" && (set isModelProcessed=1) || (set isModelProcessed=)
    if not defined isModelProcessed (
      echo Processing !modelName! w/ automatic inputs...
      @echo on
      "%bmd2gltfBasePath%bmd2gltf.exe" automatic --out "!modelBasePath!" "%extraArgs%"
      @echo off
    )
    if defined isModelProcessed (
      echo Model already processed for !modelName!
    )
  )

  :: Merges models + animations w/ manual inputs (not used yet...)
  if defined modelFile if defined animFiles (
    set modelBasePath=%outBasePath%!modelName!

    echo Processing !modelName! w/ manual inputs...
    @echo on
    "%bmd2gltfBasePath%bmd2gltf.exe" manual --out "!modelBasePath!" --bmd "!modelFile!" --bcx "!animFiles!" "%extraArgs%"
    @echo off
  )
)

popd
pushd "%hierarchyPath%"

:: Processes the Pikmin/Captain models
set pikiBasePath=user\Kando\piki\pikis.szs 0.rarc_dir\designer

set pikiAnimationsBasePath=!pikiBasePath!\motion
set pikiAnimationFetchCmd=dir /b "!pikiAnimationsBasePath!"
set pikiAnimations=
for /f "tokens=*" %%a in ('!pikiAnimationFetchCmd!') do (
  set pikiAnimations=!pikiAnimations! "!pikiAnimationsBasePath!\%%a"
)

set pikiPikminBasePath=!pikiBasePath!\piki_model
set pikminModelNames=piki_p2_black piki_p2_blue piki_p2_red piki_p2_white piki_p2_yellow
for %%m in (%pikminModelNames%) do (
  set modelName=%%m
  set modelBasePath=%outBasePath%!modelName!
  
  >nul 2>nul dir /a-d "!modelBasePath!\*" && (set isModelProcessed=1) || (set isModelProcessed=)
  if not defined isModelProcessed (
    echo Processing !modelName! w/ manual inputs...
    @echo on
    "%bmd2gltfBasePath%bmd2gltf.exe" manual --out "!modelBasePath!" --bmd "!pikiPikminBasePath!\!modelName!.bmd" --bcx !pikiAnimations! "%extraArgs%"
    @echo off
  )
  if defined isModelProcessed (
    echo Model already processed for !modelName!
  )
)

set pikiOrimaBasePath=!pikiBasePath!\orima_model
set orimaModelNames=orima1 orima3 syatyou
for %%m in (%orimaModelNames%) do (
  set modelName=%%m
  set modelBasePath=%outBasePath%!modelName!
  
  >nul 2>nul dir /a-d "!modelBasePath!\*" && (set isModelProcessed=1) || (set isModelProcessed=)
  if not defined isModelProcessed (
    echo Processing !modelName! w/ manual inputs...
    @echo on
    "%bmd2gltfBasePath%bmd2gltf.exe" manual --out "!modelBasePath!" --bmd "!pikiOrimaBasePath!\!modelName!.bmd" --bcx !pikiAnimations! "%extraArgs%"
    @echo off
  )
  if defined isModelProcessed (
    echo Model already processed for !modelName!
  )
)



:: Backs out from hierarchy
popd



echo Done!

pause