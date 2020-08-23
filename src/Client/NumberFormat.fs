// ts2fable 0.8.0
module rec moduleName
open System
open Fable.Core
open Fable.Core.JS
open Browser.Types
open Fable.Import

type Array<'T> = System.Collections.Generic.IList<'T>

let [<Import("*","module")>] ``react-number-format``: React_number_format.IExports = jsNative

module React_number_format =

    type [<AllowNullLiteral>] IExports =
        abstract unsupported_pattern: React.InputHTMLAttributes<HTMLInputElement>
        abstract NumberFormat: NumberFormatStatic

    type InputAttributes =
        obj

    type [<AllowNullLiteral>] NumberFormatState =
        abstract value: string option with get, set
        abstract numAsString: string option with get, set

    type [<AllowNullLiteral>] NumberFormatValues =
        abstract floatValue: float option with get, set
        abstract formattedValue: string with get, set
        abstract value: string with get, set

    type [<AllowNullLiteral>] FormatInputValueFunction =
        [<Emit "$0($1...)">] abstract Invoke: inputValue: string -> string

    type [<AllowNullLiteral>] SyntheticInputEvent =
        inherit React.SyntheticEvent<HTMLInputElement>
        abstract target: HTMLInputElement
        abstract data: obj option with get, set

    type [<AllowNullLiteral>] NumberFormatProps =
        inherit InputAttributes
        abstract thousandSeparator: U2<bool, string> option with get, set
        abstract decimalSeparator: U2<bool, string> option with get, set
        abstract thousandsGroupStyle: NumberFormatPropsThousandsGroupStyle option with get, set
        abstract decimalScale: float option with get, set
        abstract fixedDecimalScale: bool option with get, set
        abstract displayType: NumberFormatPropsDisplayType option with get, set
        abstract prefix: string option with get, set
        abstract suffix: string option with get, set
        abstract format: U2<string, FormatInputValueFunction> option with get, set
        abstract removeFormatting: (string -> string) option with get, set
        abstract mask: U2<string, ResizeArray<string>> option with get, set
        abstract value: U2<float, string> option with get, set
        abstract defaultValue: U2<float, string> option with get, set
        abstract isNumericString: bool option with get, set
        abstract customInput: React.ComponentType<obj option> option with get, set
        abstract allowNegative: bool option with get, set
        abstract allowEmptyFormatting: bool option with get, set
        abstract allowLeadingZeros: bool option with get, set
        abstract onValueChange: (NumberFormatValues -> unit) option with get, set
        /// these are already included in React.HTMLAttributes<HTMLInputElement>
        /// onKeyDown: Function;
        /// onMouseUp: Function;
        /// onChange: Function;
        /// onFocus: Function;
        /// onBlur: Function;
        abstract ``type``: NumberFormatPropsType option with get, set
        abstract isAllowed: (NumberFormatValues -> bool) option with get, set
        abstract renderText: (string -> React.ReactNode) option with get, set
        abstract getInputRef: U2<(HTMLInputElement -> unit), React.Ref<obj option>> option with get, set
        abstract allowedDecimalSeparators: Array<string> option with get, set
        [<Emit "$0[$1]{{=$2}}">] abstract Item: key: string -> obj option with get, set

    type [<AllowNullLiteral>] NumberFormat =
        inherit React.Component<NumberFormatProps, obj option>

    type [<AllowNullLiteral>] NumberFormatStatic =
        [<Emit "new $0($1...)">] abstract Create: unit -> NumberFormat

    type [<StringEnum>] [<RequireQualifiedAccess>] NumberFormatPropsThousandsGroupStyle =
        | Thousand
        | Lakh
        | Wan

    type [<StringEnum>] [<RequireQualifiedAccess>] NumberFormatPropsDisplayType =
        | Input
        | Text

    type [<StringEnum>] [<RequireQualifiedAccess>] NumberFormatPropsType =
        | Text
        | Tel
        | Password