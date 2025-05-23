﻿#pragma checksum "..\..\..\..\Views\DesignerView.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "891DF9C5D7F4B4C9602828BCD1F6C1184BF46541"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using PinPoint.UI.Views;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace PinPoint.UI.Views {
    
    
    /// <summary>
    /// DesignerView
    /// </summary>
    public partial class DesignerView : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 35 "..\..\..\..\Views\DesignerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas CrosshairPreview;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\..\..\Views\DesignerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Path CrosshairPath;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\..\..\Views\DesignerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox StyleComboBox;
        
        #line default
        #line hidden
        
        
        #line 91 "..\..\..\..\Views\DesignerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider SizeSlider;
        
        #line default
        #line hidden
        
        
        #line 105 "..\..\..\..\Views\DesignerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider ThicknessSlider;
        
        #line default
        #line hidden
        
        
        #line 119 "..\..\..\..\Views\DesignerView.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider OpacitySlider;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.3.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/PinPoint.UI;component/views/designerview.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\DesignerView.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.3.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.CrosshairPreview = ((System.Windows.Controls.Canvas)(target));
            return;
            case 2:
            this.CrosshairPath = ((System.Windows.Shapes.Path)(target));
            return;
            case 3:
            this.StyleComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 62 "..\..\..\..\Views\DesignerView.xaml"
            this.StyleComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.StyleComboBox_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 82 "..\..\..\..\Views\DesignerView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ChooseColor_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.SizeSlider = ((System.Windows.Controls.Slider)(target));
            
            #line 93 "..\..\..\..\Views\DesignerView.xaml"
            this.SizeSlider.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.SizeSlider_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 6:
            this.ThicknessSlider = ((System.Windows.Controls.Slider)(target));
            
            #line 107 "..\..\..\..\Views\DesignerView.xaml"
            this.ThicknessSlider.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.ThicknessSlider_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 7:
            this.OpacitySlider = ((System.Windows.Controls.Slider)(target));
            
            #line 121 "..\..\..\..\Views\DesignerView.xaml"
            this.OpacitySlider.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.OpacitySlider_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 134 "..\..\..\..\Views\DesignerView.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ApplySettings_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

