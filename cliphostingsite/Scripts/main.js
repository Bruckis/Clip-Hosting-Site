/* ========================================================= 
 *                      Profile scripts
 * =========================================================
 * 
 * Username change
 * 
 * */

$('#username').click(function () {
    var username = $(this).text();
    $(this).html('');
    $('<input></input>')
        .attr({
            'type': 'text',
            'name': 'username',
            'id': 'username',
            'size': '30',
            'value': Username
        })
        .appendTo('#username');
    $('#txt_username').focus();
});

$(document).on('blur', '#txt_username', function () {
    var username = $(this).val();
    $.ajax({
        type: 'post',
        url: '/clip/chgusrn',
        data: username,
        success: function () {
            $('#username').text(username);
            alert("Username has been changed")
        }
    });
});