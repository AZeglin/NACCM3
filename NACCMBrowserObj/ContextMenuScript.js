// @ts-check

<script language="Javascript">

function __showContextMenu( menu )
{
	var menuOffset = 2
    menu.style.left = window.event.x - menuOffset;
    menu.style.top = window.event.y - menuOffset;
    menu.style.display = "";
  
    
    // added this line of code 5/28/2015
 //   window.event.returnValue = false;

    // added code 5/28/2015 prior it was just the cancelBubble line
//    if (typeof window.event.stopPropagation != "undefined") {
 //       window.event.stopPropagation();
 //   } else {
        window.event.cancelBubble = true;
 //   }

    
    return false;
}

function __trapESC( menuName )
{
    alert( "1" );
//    var menu = document.getElementById( menuName );
//	var key = window.event.keyCode;
//	if( key == 27 )
//	{
//		menu.style.display = 'none';
//	}
//    alert( "2" );
}

function HideMenuOnClick( menuName )
{
    var menu = document.getElementById( menuName );
    menu.style.display = 'none';
}

function TestNewClickEvent( e )
{
    if( e.pointerType ){ 
        alert( evt.pointerType ); 
    } 
    else { 
        alert('TestNewClickEvent'); 
    }             
}

function GetEventTrigger( e )
{
    var et = '';
    if( !e && window.event.srcElement ) 
        et = window.event.srcElement;
    else if( e.currentTarget )
        et = e.currentTarget;
    else if( e.target )
        et = e.target;
    if( et.nodeType && et.nodeType == 3 )
        et = et.parentNode;
    return et;
}

//function __showContextMenuRightClick( menu, rowIndex, hiddenField )
//{
//    e = window.event;
//     
//	var menuOffset = 2;
//    
//    if( e.button == 2 ) // right button in ie
//    {    
////        if( hiddenField != null )
////        {
////            hiddenField.value = rowIndex;
////        }
//        
//        menu.style.left = window.event.x - menuOffset;
//        menu.style.top = window.event.y - menuOffset;
//        menu.style.display = "";
//        window.event.cancelBubble = true;
//    }
// 
//    return false;
//}

function __showContextMenuRightClick( container, menuName, rowIndex, hiddenFieldName )
{
    var e = window.event;
     
    var menuOffset = 2;
	var menuLeft = "0px";
    var menuTop = "0px";
    var hiddenField = document.getElementById( hiddenFieldName );
    var menu = document.getElementById( menuName );

    menuLeft = ( window.event.x + menuOffset ) + "px";
    menuTop =  ( window.event.y + menuOffset ) + "px";

    if (e.button == 2 )     //         if( e.button == 2 ) // right button in ie
    {    
        if( hiddenField != null )
        {
            hiddenField.value = rowIndex;
        }
        
      //  alert( 'x=' + window.event.x + ' y=' + window.event.y ); ok

        if( menu != null )
        { 
            menu.style.left = menuLeft;
            menu.style.top = menuTop;
       //     menu.animate({top:menuTop, left:menuLeft},{duration:500,queue:false}); not tested
            menu.style.display = "";
        }
     
        // added this line of code 5/28/2015
   //     window.event.returnValue = false;

        // added code 5/28/2015 prior it was just the cancelBubble line
  //      if (typeof window.event.stopPropagation != "undefined") {
  //          window.event.stopPropagation();
  //      } else {
        //    window.event.cancelBubble = true;
   //     }

    }
 
    return true;
}

//function GetScreenCordinates( obj ) 
//{
//    var p = {};
//    p.x = obj.offsetLeft;
//    p.y = obj.offsetTop;

//    while (obj.offsetParent) 
//    {
//        p.x = p.x + obj.offsetParent.offsetLeft;
//        p.y = p.y + obj.offsetParent.offsetTop;

//        if (obj == document.getElementsByTagName("body")[0]) 
//        {
//            break;
//        }
//        else 
//        {
//            obj = obj.offsetParent;
//        }
//    }

//    return p;
//}

//function GetScreenCordinates( obj ) 
//{
//    var p = {};
//    p.x = obj.screenLeft;
//    p.y = obj.screenTop;

//    return p;
//}
</script>
