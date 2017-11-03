

function bookAddNewPublisher(newPublisher) {
    $.ajax({
        type: "POST",
        url: "Add.aspx/AsyncAddPublisher",
        data: '{name: "' + newPublisher + '" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (partialViewResult) {
            BookPublisherAddToSuggestions(partialViewResult.d.Id, partialViewResult.d.Name);
        },
        failure: function (response) {
            alert("Eroare de server, te rugăm contactează administratorul de sistem");
        }
    })
}

function bookAddNewBookDomain(newDomain) {
    $.ajax({
        type: "POST",
        url: "Add.aspx/AsyncAddDomain",
        data: '{name: "' + newDomain + '" }',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (partialViewResult) {
            BookDomainAddToSuggestions(partialViewResult.d.Id, partialViewResult.d.Name);
        },
        failure: function (response) {
            alert("Eroare de server, te rugăm contactează administratorul de sistem");
        }
    })
}

function removeDiv(thisDiv) {
    thisDiv.remove();
}


