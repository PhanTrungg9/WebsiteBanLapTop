$(document).ready(function () {
    ShowCount();
    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var conf = confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?');
        if (conf == true) {
            $.ajax({
                url: '/ShoppingCart/Delete',
                type: 'POST',
                data: { id: id },
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

                        //$('#checkout_items').html(rs.Count);
                        //$('#trow_' + id).remove();
                        alert('Số lượng đã được cập nhật');
                        window.location.href = window.location.href; 
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

