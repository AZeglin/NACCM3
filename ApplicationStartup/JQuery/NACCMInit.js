/* NACCMInit.js - Loaded from ScriptManager scripts collection */
/* must use Control.ClientID to reference controls inside an include file */




function Nada(){

    if (typeof (postbackElement) == "undefined") {
        return;
    }
    alert('NACCMMasterPageLoaded A');
    var rebateScrollValue = $get("rebateScrollPos").value;
    alert('NACCMMasterPageLoaded B');
    if (postbackElement.id.toLowerCase().indexOf('rebategridview') > -1) {
        alert('NACCMMasterPageLoaded B 1');
        $get("RebateGridViewDiv").scrollTop = rebateScrollValue;
        alert('NACCMMasterPageLoaded B 2');

    }
    alert('NACCMMasterPageLoaded C');
    highlightRebateRow();
    alert('NACCMMasterPageLoaded end');

}

function saveClientScreenResolutionInfo() {
    document.forms[0].ClientScreenHeight.value = screen.height;
    document.forms[0].ClientScreenWidth.value = screen.width;      
}