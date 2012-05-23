(function() {
  $(function() {
    return $("form.autosubmit").find("input, select").change(function() {
      return $(this).parents("form").submit();
    });
  });
}).call(this);
