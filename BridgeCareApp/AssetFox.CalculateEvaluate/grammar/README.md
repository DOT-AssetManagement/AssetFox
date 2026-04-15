This project uses [ANTLR][antlr] to provide a .NET API that translates strings
in a simple number/boolean-based expression language named "Calculate-Evaluate"
("CE language" henceforth) to dynamically compiled .NET delegates.

To build this project, you don't need anything beyond the dependencies already
specified within the project metadata. The build process should take care of
acquiring those dependencies automatically.

To update the ANTLR package dependency:

1. Update the ANTLR NuGet package to the latest version.
2. Download the latest `antlr-X.Y.Z-complete.jar` file from the ANTLR project
   website.
3. Replace the corresponding JAR file in this folder with the downloaded file.
4. Edit the `antlr.bat` to use the correct JAR file name.
5. In a terminal, run `grammar2code.bat`.
6. If the ANTLR package version update traversed a major version (e.g. v4.x to
   v5.x rather than v4.13.x to v4.14.x), then you may need to perform further
   updates in the custom parser code (i.e. the code that uses the
   ANTLR-generated code).

To update the CE language grammar, we recommend VS Code with a suitable
ANTLR/Java environment. To set up such an environment, you can do the following:

1. Ensure you have a correct [VS Code with Java][vscode-java] environment by
   downloading and running the "Visual Studio Code Java Pack Installer".
2. Install the ["ANTLR4 grammar syntax support" extension for VS
   Code][vscode-antlr].

Then, to update the grammar:

3. Within VS Code, edit this project's grammar files (*.g4) as needed.
   - To debug the grammar, please refer to the documentation of the
     aforementioned VS Code extension.
4. Run this project's `grammar2code.bat` batch file.

After updating the grammar, depending on the nature of your update, you may need
to then update the non-generated code in this project.

[antlr]: https://www.antlr.org/
[vscode-java]: https://code.visualstudio.com/docs/languages/java
[vscode-antlr]: https://marketplace.visualstudio.com/items?itemName=mike-lischke.vscode-antlr4
