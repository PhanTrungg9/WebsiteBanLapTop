$(document).ready(function () {
    ShowCount();
    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');

        Swal.fire({
            title: 'Xác nhận xóa?',
            text: "Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/ShoppingCart/Delete',
                    type: 'POST',
                    data: { id: id },
                    success: function (rs) {
                        if (rs.Success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'Đã xóa!',
                                text: 'Sản phẩm đã được xóa',
                                showConfirmButton: false,
                                timer: 1500,
                                timerProgressBar: true,
                            }).then(() => {
                                $('#checkout_items').html(rs.Count);
                                window.location.href = window.location.href;
                            });
                        }
                    },
                    error: function (xhr, status, error) {
                        Swal.fire({
                            icon: 'error',
                            title: 'Lỗi!',
                            text: 'Không thể xóa sản phẩm',
                            confirmButtonText: 'OK'
                        });
                    }
                });
            }
        });
    });
    $('body').on('click', '.btnDeleteAll', function (e) {
        e.preventDefault();
        var conf = confirm('Bạn có chắc muốn xóa hết sản phẩm này khỏi giỏ hàng?');
        if (conf == true) {
            $.ajax({
                url: '/ShoppingCart/DeleteAll',
                type: 'POST',
                //data: { id: id },
                success: function (rs) {
                    if (rs.Success) {
                        $('#checkout_items').html(rs.Count);
                        //$('#trow_' + id).remove();
                        window.location.href = window.location.href;
                    }
                }
            });
        }
    });
    $('body').on('click', '.btnUpdate', function (e) {
        e.preventDefault();
        var id = $(this).data("id");
        var quantity = $('#Quantity_' + id).val();
            $.ajax({
                url: '/ShoppingCart/Update',
                type: 'POST',
                data: { id: id, quantity: quantity },
                success: function (rs) {
                    if (rs.Success) {
                        alert('Số lượng đã được cập nhật');
                        window.location.href = window.location.href;
                    }
                    else {
                        if (rs.Message != null) {
                            alert(rs.Message);
                        }
                    }
                }
            });
    });
    function ShowCount() {
        $.ajax({
            url: '/ShoppingCart/ShowCount',
            type: 'GET',
            success: function (rs) {
                $('#checkout_items').html(rs.Count);
            }
        });
    }
});

