Sys.Application.add_init(NACCMEditAppInit);

function NACCMEditAppInit(sender) {
    Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(NACCMEditBeginRequest);
    Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(NACCMEditPageLoaded);
}

var postbackElement;

function NACCMEditBeginRequest(sender, args) {
    postbackElement = args.get_postBackElement();
}

function NACCMEditPageLoaded(sender, args) {
    if (typeof (postbackElement) == "undefined") {
        return;
    }

    var itemScrollValue = $get("scrollPos").value;


    if (postbackElement.id.toLowerCase().indexOf('rebategridview') > -1) {
        $get("RebateGridViewDiv").scrollTop = itemScrollValue;

    }

    highlightRebateRow();
}


function checkTextAreaMaxLength(textBox, e, length) {

    var mLen = textBox["MaxLength"];
    if (null == mLen)
        mLen = length;

    var maxLength = parseInt(mLen);
    if (!checkSpecialKeys(e)) {
        if (textBox.value.length > maxLength - 1) {
            if (window.event)//IE
                e.returnValue = false;
            else//Firefox
                e.preventDefault();
        }
    }
}
function checkSpecialKeys(e) {
    if (e.keyCode != 8 && e.keyCode != 46 && e.keyCode != 37 && e.keyCode != 38 && e.keyCode != 39 && e.keyCode != 40)
        return false;
    else
        return true;
}

function setItemScrollForRestore(divToScroll) {
    if (divToScroll != "0") {
        $get("scrollPos").value = divToScroll.scrollTop;
    }
}

function setItemScrollOnChange(newPositionValue) {
    $get("scrollPos").value = newPositionValue;
    $get("RebateGridViewDiv").scrollTop = newPositionValue;
}


function highlightRebateRow() {
    var selectedRowIndex = $get("highlightedRebateRow").value;
    if (selectedRowIndex == -1) {
        return;
    }

    var rebateGridView = $get("RebateGridView");
    var currentSelectedRow = null;
    if (rebateGridView != null) {
        currentSelectedRow = rebateGridView.rows[selectedRowIndex];
    }
    if (currentSelectedRow != null) {
        currentSelectedRow.className = 'RebateSelectedCellStyle';
    }
}

function setRebateHighlightedRowIndexAndOriginalColor(rowIndex, originalColor) {
    $get("highlightedRebateRow").value = rowIndex;
    $get("highlightedRebateRowOriginalColor").value = originalColor;

    highlightRebateRow();
}

function presentConfirmationMessage(msg) {
    $get("confirmationMessageResults").value = confirm(msg);
}

function presentPromptMessage(msg) {
    $get("promptMessageResults").value = prompt(msg, "");
}