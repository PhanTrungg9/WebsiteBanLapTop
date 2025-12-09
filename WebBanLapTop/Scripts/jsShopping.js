$(document).ready(function () {
    ShowCount();
    var updateTimeout;

    // Tự động cập nhật khi thay đổi số lượng
    $('body').on('input change', 'input[id^="Quantity_"]', function () {
        var $input = $(this);
        var id = $input.attr('id').split('_')[1];
        var quantity = $input.val();

        // Clear timeout cũ
        clearTimeout(updateTimeout);

        // Đợi 800ms sau khi user ngừng nhập mới gọi API
        updateTimeout = setTimeout(function () {
            $.ajax({
                url: '/ShoppingCart/Update',
                type: 'POST',
                data: { id: id, quantity: quantity },
                success: function (rs) {
                    if (rs.Success) {
                        // Reload lại trang để cập nhật tổng tiền
                        window.location.href = window.location.href;
                    }
                    else {
                        if (rs.Message != null) {
                            alert(rs.Message);
                            window.location.href = window.location.href;
                        }
                    }
                }
            });
        }, 800);
    });
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

