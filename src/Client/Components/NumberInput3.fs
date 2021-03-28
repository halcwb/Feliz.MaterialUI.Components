namespace Components

module NumberInput3 =

    open System
    open Elmish
    open Feliz
    open Feliz.UseElmish
    open Feliz.MaterialUI
    open Fable.Core
    open Fable.Core.JsInterop
    open Browser

    type NumberFormatValues =
        { formattedValue: string
          value: string
          floatValue: float }

    [<Erase>]
    type numberFormat =
        static member inline customInput (elem: ReactElement) = Interop.mkAttr "customInput" elem
        static member inline format (fmt: string) = Interop.mkAttr "format" fmt        
        static member inline onValueChange (handler: NumberFormatValues -> unit) = Interop.mkAttr "onValueChange" handler
        static member inline decimalSeparator (value: char) = Interop.mkAttr "decimalSeparator" value
        static member inline decimalSeparator (value: string) = Interop.mkAttr "decimalSeparator" value
        static member inline thousandSeparator (value: char) = Interop.mkAttr "thousandSeparator" value
        static member inline thousandSeparator (value: string) = Interop.mkAttr "thousandSeparator" value

    type React with
        static member inline numberformat (props: IReactProperty list) = 
            Interop.reactElement (importDefault "react-number-format") (createObj !!props)

    type State =
        { Error: bool
          UserInput: string }

    type Msg =
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
        { Error = false
          UserInput = "" }
        , Cmd.none

    let private update (props: Props) msg state =
        let tryParse (s : string) =
            match Double.TryParse(s) with
            | true, f   -> f |> Some
            | false, _  -> None

        let isErr (s: string) =
            if s.Trim() = "" then true
            else
                match s |> tryParse, props.min, props.max with
                | None, _, _ -> false
                | Some _, None, None -> true
                | Some v, Some min, None -> v >= min
                | Some v, None, Some max -> v <= max
                | Some v, Some min, Some max -> v >= min && v <= max
            |> not

        match msg with
        | ChangeValue s -> { state with Error = s |> isErr; UserInput = s }, Cmd.none
        | EndOfInput ->
            let state = { state with Error = state.UserInput |> isErr }

            state,
            Cmd.ofSub (fun _ -> state.UserInput |> props.dispatch)


    let private useStyles err =
        Styles.makeStyles (fun styles theme ->
            {| field = styles.create [
                   style.minWidth (theme.spacing 14)
                   style.marginTop (theme.spacing 1)
               ]
               input = styles.create [
                   if not err then style.color (theme.palette.primary.main)
                   else style.color (theme.palette.error.main)
               ]
               label = styles.create [
                   style.fontSize (theme.typography.fontSize - 15.)
                   style.paddingRight (theme.spacing 2)
               ]
            |})

    let private defaultProps: Props =
        {| min = None
           max = None
           step = 1.
           label = ""
           adorn = ""
           dispatch = (fun (s: string) -> ()) |}

    let private comp = React.functionComponent("numericinput", fun (props: Props) ->
        let state, dispatch = React.useElmish (init, update props, [||])
        let classes = (useStyles state.Error) ()

        React.numberformat [
            prop.className classes.field
            textField.error state.Error
            textField.label (
                Mui.typography [
                    typography.variant.body2
                    typography.children [ props.label ]
                ]
            )

            textField.value state.UserInput
            textField.onChange (ChangeValue >> dispatch)

            let isFF =
                navigator.userAgent.ToLower().Contains("firefox")
            if not isFF then textField.type' "text"

            textField.size.small
            textField.InputProps [
                input.inputProps [
                    prop.step props.step
                    match props.min with
                    | Some m -> prop.min m
                    | None -> prop.min 0.
                    match props.max with
                    | Some m -> prop.max m
                    | None -> ()
                ]
                     
                prop.className classes.input
                input.endAdornment (
                    Mui.inputAdornment [
                        inputAdornment.position.end'
                        inputAdornment.children [
                            Mui.typography [
                                typography.color.textSecondary
                                typography.variant.body2
                                typography.children [ props.adorn ]
                            ]
                        ]
                    ]
                )
            ]
            numberFormat.customInput (import "TextField" "@material-ui/core")
            numberFormat.decimalSeparator ','
            numberFormat.thousandSeparator '.'
        ])

    let render label adorn dispatch =
        comp
            {| defaultProps with
                   label = label
                   adorn = adorn
                   dispatch = dispatch |}

    let renderWithProps props = comp (props)