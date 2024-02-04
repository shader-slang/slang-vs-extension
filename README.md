# Slang Visual Studio Extension
This is the official **Visual Studio** extension for the Slang shading language.
The extension works with Visual Studio 2022 version 17.8 or later.
For the Visual Studio **Code** extension, please check out https://github.com/shader-slang/slang-vscode-extension.

Implemented features:
 - Auto completion
 - Semantic highlighting
 - Quick info
 - Signature help
 - Go to definition
 - Diagnostics
 - Symbol navigation
 - Auto formatting
 - Commenting/Uncommenting

## Configurations
You can use a configuration file named "slangdconfig.json" in the same or parent directory of the source file to define macros or search paths for the intellisense engine. The config json follows the same syntax as the settings json of the [Visual Studio Code extension](https://github.com/shader-slang/slang-vscode-extension).

Here is an example `slangdconfig.json` file:
```
{
    "slang.predefinedMacros": [
        "MY_MACRO",
        "MY_VALUE_MACRO=1"
    ],
    "slang.additionalSearchPaths": [
        "include/",
        "c:\\external-lib\\include"
    ],
    "slang.enableCommitCharactersInAutoCompletion": "on"
}
```

## Configuring auto formatting with `.clangformat`
This extension will attempt to discover `.clangformat` files from the current and parent directories of
the source file being edited to format the code. It recognizes the configurations in the C# language section.
Alternatively, you can provide `"slang.format.clangFormatStyle"` setting in `slangdconfig.json` to specify
your format configuration inline, for example:
```
// slangdconfig.json:
{
    "slang.predefinedMacros": ["MY_MACRO"],
    ...
    ""slang.format.clangFormatStyle": "Microsoft"
}
```

## Acknowledgements

Special thanks to Alex Camaño for creating the initial version of this extension.
