namespace Components

open Browser.Types

/// Numeric input component that keeps track of min, max and step values.
/// Works well with Safari and Chrome. Has a problem with Firefox.
/// The problem is that in Firefox, the numeric input box accepts non numerical entries but totally
/// ignores this, so, also doesn't trigger any events.
module NumberInput =
    open System

    open Elmish
    open Thoth.Elmish
    open Feliz
    open Feliz.UseElmish
    open Feliz.MaterialUI
    open Fable.MaterialUI.Icons
    open Fable.Core
    open Fable.Core.JsInterop
    open Browser

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
