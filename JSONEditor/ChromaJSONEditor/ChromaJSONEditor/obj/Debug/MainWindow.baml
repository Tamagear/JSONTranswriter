   M S B A M L   `   `   `  ????K  GChromaJSONEditor, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:ChromaJSONEditor.ToolsChromaJSONEditor.Tools  5   	   3clr-namespace:ChromaJSONEditorChromaJSONEditor  5   	   ^ ZICSharpCode.AvalonEdit, Version=6.2.0.78, Culture=neutral, PublicKeyToken=9cc39be672370310oLclr-namespace:ICSharpCode.AvalonEdit.Editing;assembly=ICSharpCode.AvalonEditICSharpCode.AvalonEdit.Editing 5	   	   sNclr-namespace:ICSharpCode.AvalonEdit.Rendering;assembly=ICSharpCode.AvalonEdit ICSharpCode.AvalonEdit.Rendering 5
   	   !    ChromaJSONEditor.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   6tools$clr-namespace:ChromaJSONEditor.Tools     5   	   0localclr-namespace:ChromaJSONEditor     5   	   ?
avalonEdit.http://icsharpcode.net/sharpdevelop/avalonedit  5   	   ZeditingLclr-namespace:ICSharpCode.AvalonEdit.Editing;assembly=ICSharpCode.AvalonEdit  5	   	   ^	renderingNclr-namespace:ICSharpCode.AvalonEdit.Rendering;assembly=ICSharpCode.AvalonEdit  5
   	   -   5          ? Title$  CHROMA Card Editor??5   	    ? WindowStartupLocation$ CenterScreen=?5   	    ? WindowStyle$ SingleBorderWindow=?6.    ? Icon$ Graphics/icon.png??5   	   $
??1620??5   	   $	??920??6   .??5      ? .+?5   
   ?? $??Center=?6   $??Center=?6-   $??UniformToFill=?6H   $??Graphics/background.png??6`   >? 5   
   -   5        5      ? 5   
   .+?5      ?? .+?5      ?? $??Cards??6   $??25(?6(   $	??300??66   $
??Left=?6B   $??Oblique$?6]   
???????6q   ? 5      $	??300??6   $
??Left=?6(   .+?5      ?? -   5         ??Name$ CardSearchTextBox??5      $	??200??69   $
??Left=?6E   $??Search Card...??6`   $??5q?6v   ?? 5      -   5        $??Search??5      $??5q?6.   $??80??69   6D   5      ?? 5      -   5         ??Name$ CardSearchListBox??5      $	??300??6b   $
??Left=?6n   $	??775??6?   6?   ?? 5      -   5        $
??Left=?5      $	??295??66   $??5q?6B   .@?5      ??  ?? 
IsSelected . .??65   
All Colors6A   ?? 5      .??6#   Red6(   ?? 5       .??6#   Purple6+   ?? 5!      .??6#   Green6*   ?? 5"      .??6#   Blue6)   ?? 5#      .??6#   Black6*   5$      5%      5&      ? 5'   
   .+?5)      ? $
??Left=?6   $	??Top=?63   $??
421,45,0,0q?6K   .+?5+      ?? -   5        $??Save Current Card??5+      $	??200??65   $??5q?6A   6L   ?? 5,      -   5        $??Save All??5,      $	??200??6,   $??5q?68   6C   ?? 5-      -	   5        $
??Load??5-      $	??200??6(   $??5q?64   6?   ?? 5.      -
   5        $??New Card??5.      $	??200??6,   $??5q?68   6C   ?? 5/      -   5        $??Delete Current Card??5/      $	??200??67   $??5q?6C   6N   50      '  !ICSharpCode.AvalonEdit.TextEditor  51      -   5          Name$ JSONEditorEditorBox??51   $   $
??1268??6A   $	??758??6N   $??Right=?6[   $	??Top=?6w   ???#181818
???????   Text$ 
Loading...??6?   $	   HorizontalScrollBarVisibility$
	 Auto=?6?   $??	0,85,20,0q?6?   
   WordWrap
 . 64  d? 52      -   5         d?Name$ ShowAttributeToggleButton??52      $	??200??6>   $??Show All Attributes??6J   $
??Left=?6h   $??Bottom=?6?   $??	332,0,0,6q?6?   6?   ?? 53      -   5        $W??QGenerate Deck Atlas from .txt File (Requires Python + Active Internet Connection)??53      $
??Left=?6q   $??Bottom=?6?   $	??500??6?   $??	696,0,0,6q?6?   6?   ?? 54      -   5         ??Name$ VersionLabel??54      $R??LCHROMA Card Editor Version X.Y.Z | INTERNAL USE ONLY | ©Daggerbunny Studios??6*   $??Right=?6?   $??Bottom=?6?   $??Italic$?6?   $??10(?6?   $??2q?6?   
???????6?   55      56      57      