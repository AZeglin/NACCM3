using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Security.Permissions;

namespace VA.NAC.NACCMBrowser.BrowserObj
{
    // extends the GridView command field to include support for insert
    public class ExtendedCommandField : CommandField
    {
        [DefaultValue( true )]
        [Category( "Behavior" )]
        public virtual bool ShowUpdateButtonOnInsert
        {
            get
            {
                object obj = ViewState[ "ShowUpdateButtonOnInsert" ];
                if( obj != null )
                {
                    return ( ( bool )obj );
                }
                else
                {
                    return ( true );
                }
            }
            set
            {
                ViewState[ "ShowUpdateButtonOnInsert" ] = value;
                OnFieldChanged();
            }
        }

        [DefaultValue( true )]
        [Category( "Behavior" )]
        public virtual bool ShowCancelButtonOnInsert
        {
            get
            {
                object obj = ViewState[ "ShowCancelButtonOnInsert" ];
                if( obj != null )
                {
                    return ( ( bool )obj );
                }
                else
                {
                    return ( true );
                }
            }
            set
            {
                ViewState[ "ShowCancelButtonOnInsert" ] = value;
                OnFieldChanged();
            }
        }



        protected override void OnFieldChanged()
        {
            base.OnFieldChanged();
        }

        // used for cloning
        protected override DataControlField CreateField()
        {
            return base.CreateField();
        }

        public override void InitializeCell( DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex )
        {
            string index = rowIndex.ToString();
			
			if( cellType == DataControlCellType.DataCell )
			{
                // handle insert row
				if( ( rowState & DataControlRowState.Insert ) != 0 ) 
                {
                    // handle like an edit
                    //base.InitializeCell( cell, cellType, DataControlRowState.Edit, rowIndex );

                    // support Insert and CancelInsert commands, with index as the argument
                    if( ShowUpdateButtonOnInsert == true )
                    {
                        Button saveButton = new Button();
                        saveButton.Text = "Save";
                        saveButton.ID = "SaveAfterInsertButton";
                        saveButton.Click += new EventHandler( saveOnInsertButton_Click );

                        cell.Controls.Add( saveButton );

                        if( ShowCancelButtonOnInsert == true )
                        {
                            AddSeparator( cell );

                            Button cancelButton = new Button();
                            cancelButton.Text = "Cancel";
                            cancelButton.ID = "CancelAfterInsertButton";
                            cancelButton.Click += new EventHandler( cancelOnInsertButton_Click );

                            cell.Controls.Add( cancelButton );
                        }
                    }
				}
                else
                {
                    base.InitializeCell( cell, cellType, rowState, rowIndex );
                }
			} 
            else
            {
                base.InitializeCell( cell, cellType, rowState, rowIndex );
            }
		}
		
		private void AddSeparator( DataControlFieldCell cell )
		{
			if( cell.Controls.Count > 0 ) 
            {
				Literal separator = new Literal();
				separator.Text = "&nbsp;";
				cell.Controls.Add( separator );
			}
		}

        protected void saveOnInsertButton_Click( object sender, EventArgs e )
        {

        }

        protected void cancelOnInsertButton_Click( object sender, EventArgs e )
        {

        }
	}


    //        TableCell insertCommandCell = row.Cells[ InsertCommandColumnIndex ];

    //Button saveButton = new Button();
    //saveButton.Text = "Saveo";
    //saveButton.CommandName = SaveNewCommandName;
    ////       saveButton.Command += new CommandEventHandler( saveButton_Command );
    //saveButton.Click += new EventHandler( saveButton_Click );
    //saveButton.ID = "SaveAfterInsertButton";

    ////        saveButton.CommandArgument = row.RowIndex.ToString();

    //Button cancelButton = new Button();
    //cancelButton.Text = "Cancelo";
    //cancelButton.CommandName = CancelNewCommandName;
    ////       cancelButton.Command += new CommandEventHandler( cancelButton_Command );
    //cancelButton.Click += new EventHandler( cancelButton_Click );
    //cancelButton.ID = "CancelAfterInsertButton";

    ////          cancelButton.CommandArgument = row.RowIndex.ToString();

    //insertCommandCell.Controls.Add( saveButton );
    //insertCommandCell.Controls.Add( cancelButton );

}
