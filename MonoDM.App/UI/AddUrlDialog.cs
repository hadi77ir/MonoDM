using System;
using Gtk;
using MonoDM.Core;
using MonoDM.Core.UI;

namespace MonoDM.App.UI
{
    public class AddUrlDialog : Gtk.Dialog
    {
        public AddUrlDialog()
        {
	        InitializeComponent();
        }

	    private Gtk.Entry txtUrl;
	    private Gtk.Entry txtUser;
	    private Gtk.Entry txtPassword;
	    private CheckButton cbAuth;
	    protected void InitializeComponent()
	    {
		    txtUrl = new Entry();
		    txtUser = new Entry();
		    txtPassword = new Entry();
		    cbAuth = new CheckButton();

		    txtPassword.Visibility = false;
		    
		    cbAuth.Toggled += CbAuthOnToggled;
		    cbAuth.Label = "Use Authentication?";
		    
		    Frame authFrame = new Frame();
		    authFrame.Label = "Authentication";
		    VBox authframevbox = new VBox();
		    
		    authframevbox.PackStart(cbAuth, false,false, 10);
		    HBox credentialsBox = new HBox();
		    
		    credentialsBox.PackStart(new Label("Username"), false,false, 10);
		    credentialsBox.PackStart(txtUser, true,true, 10);
		    credentialsBox.PackStart(new Label("Password"), false,false, 10);
		    credentialsBox.PackStart(txtPassword, true,true, 10);
		    
		    authframevbox.PackStart(credentialsBox);
		    
		    authFrame.Add(authframevbox);
		    
		    VBox.PackStart(txtUrl, true, true, 10);
		    VBox.PackStart(authFrame, false,false, 0);

		    Resizable = false;
		    
		    ShowAll();
		    
		    DefaultResponse = ResponseType.Cancel;
		    AddButton("Cancel",ResponseType.Cancel);
		    AddButton("Ok",ResponseType.Ok);
		    Response += btnOk_Click;
	    }


	    private void CbAuthOnToggled(object sender, EventArgs e)
	    {
		    txtUser.Sensitive = cbAuth.Active;
		    txtPassword.Sensitive = cbAuth.Active;
	    }

	    public Uri Url
	    {
		    get
		    {
			    try
			    {
				    return new Uri(txtUrl.Text, UriKind.Absolute);
			    }
			    catch
			    {
				    return null;
			    }
		    }
		    set { txtUrl.Text = value.ToString(); }
	    }

	    public bool HasAuth
	    {
		    get { return cbAuth.Active; }
		    set { cbAuth.Active = value; }
	    }

	    public string Username
	    {
		    get { return txtUser.Text; }
		    set { txtUser.Text = value; }
	    }
	    public string Password
	    {
		    get { return txtPassword.Text; }
		    set { txtPassword.Text = value; }
	    }

	    private ResourceLocation rl;
	    public ResourceLocation ResourceLocation => rl;
	    private void btnOk_Click(object o, ResponseArgs args)
	    {
		    if (args.ResponseId != ResponseType.Ok)
		    {
			    Destroy();
			    return;
		    }

		    try
		    {
                rl = new ResourceLocation();
			    rl.URL = Url.ToString();
			    rl.Authenticate = HasAuth;
			    if (HasAuth)
			    {
				    rl.Login = Username;
				    rl.Password = Password;
			    }

			    rl.BindProtocolProviderType();

				if (rl.ProtocolProviderType == null)
				{
					GtkUtils.ShowMessageBox(null, "Invalid URL format, please check the location field.", MessageType.Error);

					rl = null;
					return;
				}

				Destroy();
			}
			catch (Exception)
			{
				rl = null;
				GtkUtils.ShowMessageBox(null, "Not recognized URL type.", MessageType.Error);
				Destroy();
			}    
	    }
    }
}
