var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "sAjaxSource": "/api/CompanyApi",
        "bServerSide": true,
        "bProcessing": true,
        "bSearchable": true,
        "order": [[0, 'asc']],
        "language": {
            "emptyTable": "No record found.",
            "processing":
                '<i class="fa fa-spinner fa-spin fa-3x fa-fw" style="color:#2a2b2b;"></i><span class="sr-only">Loading...</span> '
        },
        "columns": [
            { "data": "name", "width": "15%", "searchable": true },
            { "data": "streetAddress", "width": "15%", "searchable": true },
            { "data": "city", "width": "10%", "searchable": true },
            { "data": "state", "width": "10%", "searchable": true },
            { "data": "phoneNumber", "width": "15%", "searchable": true },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/Company/Upsert/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> 
                                </a>
                                <a onclick=Delete("${data}") class="btn btn-danger text-white" style="cursor:pointer">
                                    <i class="fas fa-trash-alt"></i> 
                                </a>
                            </div>
                           `;
                }, "width": "25%"
            }
        ]
    });
}

function Delete(id) {
    swal({
        title: "Are you sure you want to Delete?",
        text: "You will not be able to restore the data!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: "/api/CompanyApi/" + id,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}

//const uri = 'api/todoitems';
//let todos = [];

//function getItems() {
//    fetch(uri)
//        .then(response => response.json())
//        .then(data => _displayItems(data))
//        .catch(error => console.error('Unable to get items.', error));
//}

//function deleteItem(id) {
//    fetch(`${uri}/${id}`, {
//        method: 'DELETE'
//    })
//        .then(() => getItems())
//        .catch(error => console.error('Unable to delete item.', error));
//}

//getItems();