﻿

#pragma checksum "C:\Users\clucky\documents\visual studio 2013\Projects\Chime81\Chime81\Pages\AlbumPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "C6B1F649C2A842B6914D2FB35B5E7C79"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MusicMink.Pages
{
    partial class AlbumPage : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 59 "..\..\..\Pages\AlbumPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.HandlePlayButtonClick;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 64 "..\..\..\Pages\AlbumPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.HandleQueueButtonClick;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 69 "..\..\..\Pages\AlbumPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.HandleShuffleButtonClick;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 74 "..\..\..\Pages\AlbumPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.HandleEditButtonClick;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}

