export default function () {
    $('.modal').on('shown.bs.modal', function () {
        $(this).find('.focus-on-show').focus();
    })
}
