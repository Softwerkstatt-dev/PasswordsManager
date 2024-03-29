using System;
using System.Windows;
using System.Windows.Media;
using Passwords.API.Abstracts;
using Passwords.API.Xaml;
using System.Windows.Controls;

namespace Passwords.GUI
{
    public partial class InfoMessage : Window, IThePasswords_TheGUI_ADialog<InfoMessage.Message>
    {
        private StackPanel panel = null;
        private Label      label;

        public class Message : EntityBase<Message>
        {
            internal static InfoMessage dialog;
            public string Text {
                get { return (string)dialog.txt_Content.Content; }
                set { dialog.txt_Content.Content = value; }
            }
            public Message() : this( API.Abstracts.Status.Unknown ) { }
            public Message( Status code ) : base( code ) {
                if ( code.Code.HasFlag( ResultCode.Xaml ) ) {
                    dialog.Title = String.Format( code.GetText(), "" );
                    double width = dialog.txt_Content.Width;
                    dialog.txt_Content.Visibility = Visibility.Collapsed;
                    dialog.pnl_Content.Visibility = Visibility.Visible;
                    string xaml = (string)code.Data;
                    xaml = XamlView.StackPanel( xaml.Substring(1,xaml.Length-2) ).Replace("\\n","\n").Replace('\'','"');
                    ( ( dialog.pnl_Content.Content = System.Xaml.XamlServices.Parse(xaml) as StackPanel ) as StackPanel ).Width = width;
                    dialog.UpdateLayout();
                } else {
                    if( dialog.pnl_Content.Content != dialog.label )
                        dialog.pnl_Content.Content = dialog.label;
                    dialog.label.Height = 128;
                    dialog.pnl_Content.Visibility = Visibility.Collapsed;
                    dialog.txt_Content.Visibility = Visibility.Visible;
                    Text = string.Format( code.GetText(), code.Data );
                    dialog.UpdateLayout();
                }
            }
        }

        private Message message;

        ThePasswords_TheAPI_TheGUI ThePasswords_TheGUI_ItsDialogs_TheInterface.TheGUI { get; set; }
        IThePasswords_TheGUI_ADialog<Message>.ItsReturnAction IThePasswords_TheGUI_ADialog<Message>.TheAction { get; set; }
        IEntityBase ThePasswords_TheGUI_ItsDialogs_TheInterface.TheData { get; set; }
        public ThePasswords_TheGUI_ItsDialogs_TheInterface theDialog() { return this; }
        void ThePasswords_TheGUI_ItsDialogs_TheInterface.Returns() {
            (this as IThePasswords_TheGUI_ADialog<Message>).TheAction( TheReturnData<Message>.fromTheDialog( this ) );
        }
        void ThePasswords_TheGUI_ItsDialogs_TheInterface.TheReturnAction<T>( IThePasswords_TheGUI_ADialog<T>.ItsReturnAction onProcced ) {
            if (typeof(T) == typeof(Message)) {
                (this as IThePasswords_TheGUI_ADialog<Message>).TheAction = onProcced
                      as IThePasswords_TheGUI_ADialog<Message>.ItsReturnAction;
            } else throw new Exception("the dialog it's data type is not: " + typeof(T).Name);
        }

        public DialogReturnState Status {
            get {
                return theDialog().TheData.Status
                     ? DialogReturnState.Ok
                     : DialogReturnState.Canceled;
            }
        }


        public InfoMessage( ThePasswords_TheAPI_TheGUI main, IThePasswords_TheGUI_ADialog<Message>.ItsReturnAction action ) 
        {
            theDialog().TheGUI = main;
            theDialog().TheReturnAction( action );

            InitializeComponent();

            btn_Decline.Click += Btn_Decline_Click;
            btn_Confirm.Click += Btn_Confirm_Click;

            label = pnl_Content.Content as Label;
            Message.dialog = this;
            message = new Message( API.Abstracts.Status.NoState );
            theDialog().TheData = message;
        }

        private void Btn_Confirm_Click( object sender, RoutedEventArgs e )
        {
            if( txt_Input.Visibility == Visibility.Visible ) {
                message.Is().Status = new Status( ResultCode.Success, txt_Content.Content.ToString(), txt_Input.Text );
                txt_Input.Clear();
                txt_Input.Visibility = Visibility.Collapsed;
            } else if( pnl_Content.Visibility == Visibility.Visible ) {
                pnl_Content.Content = label;
                label.Height = 128;
                panel = null;
                pnl_Content.Visibility = Visibility.Collapsed;
                txt_Content.Visibility = Visibility.Visible;
            } else {
                message.Is().Status = API.Abstracts.Status.Success;
            } theDialog().Returns();
        }

        private void Btn_Decline_Click( object sender, RoutedEventArgs e )
        {
            txt_Input.Clear();
            txt_Input.Visibility = Visibility.Collapsed;
            btn_Decline.Visibility = Visibility.Collapsed;
            message.Is().Status = Message.Invalid.Is().Status;
            theDialog().Returns();
        }

        private void SetImage( string icon, string image )
        {
            img_Content.Source = (ImageSource)App.Current.Resources[image];
            Icon = (ImageSource)App.Current.Resources[icon];
        }

        public void Show( Status info )
        {
            message = new Message( info );
            theDialog().TheData = message;
            if ( info.Intermediate ) {
                SetImage("WARN","Gelb");
                btn_Decline.Visibility = Visibility.Visible;
                btn_Confirm.Visibility = Visibility.Visible;
                txt_Input.Visibility = Visibility.Visible;
            } else if ( info.Ok ) {
                SetImage("PASS","Green");
                btn_Decline.Visibility = Visibility.Collapsed;
                btn_Confirm.Visibility = Visibility.Visible;
            } else if ( info.Bad ) {
                SetImage("FAIL","Red");
                btn_Decline.Visibility = Visibility.Visible;
                btn_Confirm.Visibility = Visibility.Collapsed;
            } Show();
        }
    }
}
