namespace Components

open Browser.Types
open Fable.React

/// Numeric input component that keeps track of min, max and step values.
/// Works well with Safari and Chrome. Has a problem with Firefox.
/// The problem is that in Firefox, the numeric input box accepts non numerical entries but totally
/// ignores this, so, also doesn't trigger any events.
module NumberInput =
    open System
    open System.ComponentModel

    open Elmish
    open Thoth.Elmish
    open Feliz
    open Feliz.UseElmish
    open Feliz.MaterialUI
    open Fable.MaterialUI.Icons
    open Fable.Core
    open Fable.Core.JsInterop
    open Browser

    let private merge (o1: obj) (o2: obj) : obj = import "merge" "./../number-format.js"

    //[<Erase>]
    //module NumberFormat =

    //    module Props =
            
    //        type Props =

    //            //thousandSeparator	mixed: single character string or boolean true (true is default to ,)	none	Add thousand separators on number
    //            | ThousandSeparator of char
    //            //decimalSeparator	single character string	.	Support decimal point on a number
    //            | DecimalSeparator of char
    //            //thousandsGroupStyle	One of ['thousand', 'lakh', 'wan']	thousand	Define the thousand grouping style, It support three types. thousand style (thousand) : 123,456,789, indian style (lakh) : 12,34,56,789, chinese style (wan) : 1,2345,6789
    //            //decimalScale	number	none	If defined it limits to given decimal scale
    //            //fixedDecimalScale	boolean	false	If true it add 0s to match given decimalScale
    //            //allowNegative	boolean	true	allow negative numbers (Only when format option is not provided)
    //            //allowEmptyFormatting	boolean	false	Apply formatting to empty inputs
    //            //allowLeadingZeros	boolean	false	Allow leading zeros at beginning of number
    //            //prefix	String (ex : $)	none	Add a prefix before the number
    //            //suffix	String (ex : /-)	none	Add a suffix after the number
    //            //value	Number or String	null	Value to the number format. It can be a float number, or formatted string. If value is string representation of number (unformatted), isNumericString props should be passed as true.
    //            | Value of obj
    //            //defaultValue	Number or String	null	Value to be used as default value if value is not provided. The format of defaultValue should be similar as defined for the value.
    //            //isNumericString	boolean	false	If value is passed as string representation of numbers (unformatted) then this should be passed as true
    //            //displayType	String: text / input	input	If input it renders a input element where formatting happens as you input characters. If text it renders it as a normal text in a span formatting the given value
    //            //type	One of ['text', 'tel', 'password']	text	Input type attribute
    //            //format	String : Hash based ex (#### #### #### ####)
    //            //Or Function	none	If format given as hash string allow number input inplace of hash. If format given as function, component calls the function with unformatted number and expects formatted number.
    //            //removeFormatting	(formattedValue) => numericString	none	If you are providing custom format method and it add numbers as format you will need to add custom removeFormatting logic
    //            //mask	String (ex : _)	' '	If mask defined, component will show non entered placed with masked value.
    //            //customInput	Component Reference	input	This allow supporting custom inputs with number format.
    //            //onValueChange	(values) => {}	none	onValueChange handler accepts values object
    //            | OnValueChange of (obj -> unit)
    //            //isAllowed	(values) => true or false	none	A checker function to check if input value is valid or not
    //            //renderText	(formattedValue) => React Element	null	A renderText method useful if you want to render formattedValue in different element other than span.
    //            //getInputRef	(elm) => void	null	Method to get reference of input, span (based on displayType prop) or the customInput's reference. See Getting reference
    //            | GetInputRef of (obj -> unit)
    //            //allowedDecimalSeparators	array of char	none	Characters which when pressed result in a decimal separator. When missing, decimal separator and '.' are used

    [<Erase>]
    type NumberFormat =

        static member inline numberformat =
            fun props ->

                let props =
                    //[
                    //    props?value |> Props.Value
                    //    props?inputRef |> Props.GetInputRef
                    //    ',' |> Props.DecimalSeparator
                    //    '.' |> Props.ThousandSeparator

                    //    fun values ->
                    //        printfn "getting value: %A" values?value
                    //        {| target = {| value = values?value |}|}
                    //        |> props?onChange
                    //    |> Props.OnValueChange
                    //]
                    // |> keyValueList CaseRules.LowerFirst

                    {|
                        onValueChange =
                            fun values ->
                                printfn "getting value: %A" values?value
                                {| target = {| value = values?value |} |}
                        decimalSeparator = ","
                        thousandSeparator = "."
                    |}
                    |> merge props


                ofImport "default" "./../number-format.js" props []

            |> ReactElementType.ofFunction
    

    type Locale =
        | Dutch
        | English

    let tryParseFloat locale (s: string) =
        let s =
            match locale with
            | Dutch -> s.Replace(".", "").Replace(",", ".")
            | English -> s.Replace(",", "")

        match Double.TryParse(s) with
        | true, f -> Some f
        | false, _ -> None

    let tryParseDutch   = tryParseFloat Dutch
    let tryParseEnglish = tryParseFloat English

    [<Global>]
    let private Intl: obj = jsNative

    /// The debouncer keeps tracks of the bounce time
    /// before the UserInput is actually used.
    type State =
        {
            Debouncer: Debouncer.State
            Error: bool
            UserInput: string
            Number: float option
        }


    type Msg =
        | DebouncerSelfMsg of Debouncer.SelfMessage<Msg>
        | ChangeValue of string
        | EndOfInput


    type Props =
        {| min: float option
           max: float option
           step: float
           label: string
           adorn: string
           dispatch: string -> unit |}


    let private init () =
        {
            Debouncer = Debouncer.create ()
            Error = false
            UserInput = ""
            Number = None
        },
        Cmd.none


    let private update (props: Props) msg state =
        let isErr (s: string) =
            if s.Trim() = "" then
                true
            else
                match s |> tryParseDutch, props.min, props.max with
                | None, _, _ -> false
                | Some _, None, None -> true
                | Some v, Some min, None -> v >= min
                | Some v, None, Some max -> v <= max
                | Some v, Some min, Some max -> v >= min && v <= max
            |> not

        match msg with
        | ChangeValue s ->
            let debouncerModel, debouncerCmd =
                state.Debouncer
                |> Debouncer.bounce (TimeSpan.FromSeconds 1.) "end-of-input" EndOfInput

            { state with
                Error = s |> isErr
                UserInput =
                    if s = "" || s = "-" then
                        s
                    else
                        match s |> tryParseDutch with
                        | Some _ -> s
                        | None -> state.UserInput
                Number =
                    if s = "" then
                        None
                    else
                        match s |> tryParseDutch with
                        | Some n -> n |> Some
                        | None -> state.Number
                Debouncer = debouncerModel
            },

            Cmd.map DebouncerSelfMsg debouncerCmd

        | DebouncerSelfMsg debouncerMsg ->
            let debouncerModel, debouncerCmd =
                Debouncer.update debouncerMsg state.Debouncer

            { state with
                Debouncer = debouncerModel
            },
            debouncerCmd
        // End of user input has reached so know actually dispatch the
        // input to the dispatch function
        | EndOfInput ->
            let state =
                { state with
                    Error = state.UserInput |> isErr
                    UserInput =
                        match state.Number with
                        | Some n ->
                            let opts =
                                createObj [
                                    "minimumFractionDigits" ==> 0
                                    "maximumFractionDigits" ==> 20
                                ]

                            Intl?NumberFormat("nl", opts)?format(n)
                        | None -> ""
                }

            state,
            Cmd.ofSub (fun _ ->
                match state.Number with
                | Some n -> n |> string
                | None -> ""
                |> props.dispatch)


    let useStyles err =
        Styles.makeStyles (fun styles theme ->
            {|
                field =
                    styles.create [
                        style.minWidth (theme.spacing 14)
                        style.marginTop (theme.spacing 1)
                    ]
                input =
                    styles.create
                        [
                            if not err then
                                style.color (theme.palette.primary.main)
                            else
                                style.color (theme.palette.error.main)
                        ]
                label =
                    styles.create [
                        style.fontSize (theme.typography.fontSize - 15.)
                        style.paddingRight (theme.spacing 2)
                    ]
            |})


    let defaultProps: Props =
        {|
            min = None
            max = None
            step = 1.
            label = ""
            adorn = ""
            dispatch = (fun (s: string) -> ())
        |}


    let private comp =
        React.functionComponent
            ("numericinput",
             (fun (props: Props) ->
                 let state, dispatch =
                     React.useElmish (init, update props, [||])

                 let classes = (useStyles state.Error) ()

                 Mui.textField [
                     prop.className classes.field
                     textField.error state.Error
                     textField.label
                         (Mui.typography [
                             typography.variant.body2
                             typography.children [ props.label ]
                          ])

                     textField.value state.UserInput
                     textField.onChange (ChangeValue >> dispatch)

                     // dirty fix to disable number field typ in FF
                     let isFF =
                         navigator.userAgent.ToLower().Contains("firefox")
                     if not isFF then textField.type' "text"

                     textField.size.small
                     textField.InputProps [
                         // uses the react-number-format lib
                         // input.inputComponent (NumberFormat.numberformat)

                         input.inputProps [
                             prop.step props.step
                             match props.min with
                             | Some m -> prop.min m
                             | None -> prop.min 0.
                             match props.max with
                             | Some m -> prop.max m
                             | None -> ()
                         ]
                         
                         // sets the color of the input value
                         prop.className classes.input
                         // adds a unit (adornment) to a value
                         input.endAdornment
                             (Mui.inputAdornment [
                                 inputAdornment.position.end'
                                 inputAdornment.children
                                     [
                                         Mui.typography [
                                             typography.color.textSecondary
                                             typography.variant.body2
                                             typography.children [ props.adorn ]
                                         ]
                                     ]
                              ])
                     ]
                 ]))


    let render label adorn dispatch =
        comp
            ({| defaultProps with
                 label = label
                 adorn = adorn
                 dispatch = dispatch
             |})


    let renderWithProps props = comp (props)
