﻿

#pragma checksum "C:\Users\clucky\documents\visual studio 2013\Projects\Chime81\Chime81\Dialogs\EditAlbum.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "65148AEEA68D9C43525FC1984BA386B4"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MusicMink.Dialogs
{
    partial class EditAlbum : global::Windows.UI.Xaml.Controls.ContentDialog, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 10 "..\..\..\Dialogs\EditAlbum.xaml"
                ((global::Windows.UI.Xaml.Controls.ContentDialog)(target)).PrimaryButtonClick += this.ContentDialog_PrimaryButtonClick;
                 #line default
                 #line hidden
                #line 11 "..\..\..\Dialogs\EditAlbum.xaml"
                ((global::Windows.UI.Xaml.Controls.ContentDialog)(target)).SecondaryButtonClick += this.ContentDialog_SecondaryButtonClick;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 61 "..\..\..\Dialogs\EditAlbum.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.HandleLaunchFilePickerButtonClick;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 67 "..\..\..\Dialogs\EditAlbum.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.HandleGetLastFMArtButtonClick;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}

