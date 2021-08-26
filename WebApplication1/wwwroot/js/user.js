var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "sAjaxSource": "/Admin/User/GetAll",
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
            { "data": "email", "width": "15%", "searchable": true },
            { "data": "phoneNumber", "width": "15%", "searchable": true  },
            { "data": "company.name", "width": "15%", "searchable": true  },
            { "data": "role", "width": "15%", "searchable": true  },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/User/Update/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> 
                                </a>
                                <a onclick=Delete("/Admin/User/Delete/${data}") class="btn btn-danger text-white" style="cursor:pointer">
                                    <i class="fas fa-trash-alt"></i>
                                </a>
                            </div>
                           `;
                }, "width": "25%"
            }
        ],
    });
}

function Delete(url) {
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
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                    window.location.replace("/Identity/Account/Logout");
                }
            });
        }
    });
}