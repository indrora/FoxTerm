using System;
using System.Collections;
using Gtk;
using Gnome;
using Vte;
using GConf;


class FoxTerm
{
	static void Main (string[] args)
	{
		new FoxTerm (args);
	}

	Program program;
	App app;
	
	FoxTerm (string[] args)
	{
		program = new Program ("FoxTerm", "0.1", Modules.UI, args);
		app = new App ("FoxTerm", "Terminal");
		app.SetDefaultSize (600, 450);
		app.DeleteEvent += new DeleteEventHandler (OnAppDelete);
		
		GConf.Client gc_client = new GConf.Client();
		
		app.IconName = "terminal";
		
		Terminal term = new Terminal ();
		term.EncodingChanged += new EventHandler (OnEncodingChanged);
		term.CursorBlinks = true;
		term.MouseAutohide = true;
		term.ScrollOnKeystroke = true;
		term.ScrollbackLines = int.MaxValue;
		term.DeleteBinding = TerminalEraseBinding.Auto;
		term.BackspaceBinding = TerminalEraseBinding.Auto;
		term.Encoding = "UTF-8";
		
		term.FontFromString = (string)gc_client.Get("/desktop/gnome/interface/monospace_font_name");
		term.ChildExited += new EventHandler (OnChildExited);
		term.WindowTitleChanged += OnTitleChanged;
		
		
		ScrolledWindow scroll = new ScrolledWindow(null,term.Adjustment);
		scroll.Add(term);
		scroll.HscrollbarPolicy = PolicyType.Automatic;
		scroll.HScrollbar.Hide();
		
		string[] argv = Environment.GetCommandLineArgs ();
		// wants an array of "variable=value"
		string[] envv = new string[Environment.GetEnvironmentVariables ().Count];
		int i = 0;
		foreach (DictionaryEntry e in Environment.GetEnvironmentVariables ()) {
			if ((string)(e.Key) == "" || (string)(e.Value) == "")
				continue;
			string tmp = String.Format ("{0}={1}", e.Key, e.Value);
			envv[i] = tmp;
			i++;
		}
		
		int pid = term.ForkCommand (Environment.GetEnvironmentVariable ("SHELL"), argv, envv, Environment.CurrentDirectory, false, true, true);
		
		app.Contents = scroll;
		app.ShowAll ();
		program.Run ();
	}

	private void OnTitleChanged(object o, EventArgs args)
	{
		Vte.Terminal term = (Vte.Terminal)o;
		
		app.Title = (term.WindowTitle == ""? "(untitled terminal)":term.WindowTitle);
		
	}
	
	private void OnTextDeleted (object o, EventArgs args)
	{
		Console.WriteLine ("text deleted");
	}

	private void OnEncodingChanged (object o, EventArgs args)
	{
		Console.WriteLine ("encoding changed");
	}

	private void OnTextInserted (object o, EventArgs args)
	{
		Console.WriteLine ("text inserted");
	}

	private void OnChildExited (object o, EventArgs args)
	{
		// optionally we could just reset instead of quitting
		Console.WriteLine ("child exited");
		Application.Quit ();
	}

	private void OnAppDelete (object o, DeleteEventArgs args)
	{
		Application.Quit ();
	}
}
