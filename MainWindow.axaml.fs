namespace Simple_Unit_Converter

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml
open Avalonia.Interactivity
open System.Collections.Generic

type ConversionCategory =
    | Temperature
    | Length
    | Weight
    | DigitalStorage

type ConversionType = {
    Category: ConversionCategory
    FromUnit: string
    ToUnit: string
    Conversion: float -> float
}

type MainWindow() as this =
    inherit Window()

    let mutable categoryComboBox : ComboBox = null
    let mutable conversionComboBox : ComboBox = null
    let mutable inputValue : TextBox = null
    let mutable convertButton : Button = null
    let mutable resultText : TextBlock = null

    let conversions = ResizeArray [
        { Category = Temperature; FromUnit = "Celsius"; ToUnit = "Fahrenheit"; Conversion = fun c -> (c * 9.0/5.0) + 32.0 }
        { Category = Temperature; FromUnit = "Fahrenheit"; ToUnit = "Celsius"; Conversion = fun f -> (f - 32.0) * 5.0/9.0 }
        { Category = Temperature; FromUnit = "Celsius"; ToUnit = "Kelvin"; Conversion = fun c -> c + 273.15 }
        
        { Category = Length; FromUnit = "Miles"; ToUnit = "Kilometers"; Conversion = fun m -> m * 1.60934 }
        { Category = Length; FromUnit = "Kilometers"; ToUnit = "Miles"; Conversion = fun k -> k / 1.60934 }
        { Category = Length; FromUnit = "Feet"; ToUnit = "Meters"; Conversion = fun f -> f * 0.3048 }
        
        { Category = Weight; FromUnit = "Pounds"; ToUnit = "Kilograms"; Conversion = fun p -> p * 0.453592 }
        { Category = Weight; FromUnit = "Kilograms"; ToUnit = "Pounds"; Conversion = fun k -> k / 0.453592 }
        
        { Category = DigitalStorage; FromUnit = "Megabytes"; ToUnit = "Gigabytes"; Conversion = fun mb -> mb / 1024.0 }
        { Category = DigitalStorage; FromUnit = "Gigabytes"; ToUnit = "Terabytes"; Conversion = fun gb -> gb / 1024.0 }
    ]

    do this.InitializeComponent()

    member private this.InitializeComponent() =
#if DEBUG
        this.AttachDevTools()
#endif
        AvaloniaXamlLoader.Load(this)
        categoryComboBox <- this.FindControl<ComboBox>("CategoryComboBox")
        conversionComboBox <- this.FindControl<ComboBox>("ConversionComboBox")
        inputValue <- this.FindControl<TextBox>("InputValue")
        convertButton <- this.FindControl<Button>("ConvertButton")
        resultText <- this.FindControl<TextBlock>("ResultText")

        categoryComboBox.ItemsSource <- ResizeArray [
            "Temperature"
            "Length"
            "Weight"
            "DigitalStorage"
        ]
        categoryComboBox.SelectedIndex <- 0

        categoryComboBox.SelectionChanged.Add(fun _ -> this.UpdateConversionTypes())
        convertButton.Click.Add(fun _ -> this.Convert())
        inputValue.TextChanged.Add(fun _ -> this.UpdatePreview())

    member private this.UpdateConversionTypes() =
        let selectedCategory = 
            match categoryComboBox.SelectedItem with
            | :? string as s -> s
            | _ -> ""
        
        let filteredConversions = 
            conversions
            |> Seq.filter (fun ct -> string ct.Category = selectedCategory)
            |> Seq.map (fun ct -> $"{ct.FromUnit} to {ct.ToUnit}")
            |> ResizeArray
        
        conversionComboBox.ItemsSource <- filteredConversions
        if filteredConversions.Count > 0 then
            conversionComboBox.SelectedIndex <- 0

    member private this.UpdatePreview() =
        match Double.TryParse inputValue.Text with
        | true, value -> 
            try
                this.ConvertValue(value) |> ignore
            with _ -> ()
        | _ -> ()

    member private this.Convert() =
        match Double.TryParse inputValue.Text with
        | true, value -> 
            try
                let result = this.ConvertValue(value)
                resultText.Text <- result
            with ex ->
                resultText.Text <- $"Error: {ex.Message}"
        | _ ->
            resultText.Text <- "⚠️ Please enter a valid number"

    member private this.ConvertValue(value: float) =
        let conversionText = conversionComboBox.SelectedItem :?> string
        let parts = conversionText.Split([|" to "|], StringSplitOptions.None)
        let fromUnit = parts.[0]
        let toUnit = parts.[1]

        let conversion = 
            conversions
            |> Seq.find (fun ct -> ct.FromUnit = fromUnit && ct.ToUnit = toUnit)

        let convertedValue = conversion.Conversion value
        $"{value:N2} {fromUnit} = {convertedValue:N2} {toUnit}"