#MacOSStringResourceValidator

Validator for .strings files(iOS string resource file), running on Windows with GUI, .NET 4.5. 

##Rules
Currently only tow rules:
* validation of syntaxis, `"key" = "value";`
* validation of text encoding, e.g. if .strings file encoded with ASCII, but contains some chars that doesn't belong to ASCII, validator will give an error. so, **Recommended encoding is UTF-8**.

## Binary releases

* v1.0-pre https://github.com/fishtrees/MacOSStringResourceValidator_Binary/releases/tag/v1.0-pre

## TODO

* i18n

## THANKS
* @dcordero https://github.com/dcordero/Rubustrings
  * The idea came from Rubustrings. I should use Rubustrings directly on Windows, but because some ruby environment problem, I have to rewrite validators with C#. 

## License
GNU GENERAL PUBLIC LICENSE Version 3
