// ���������� ��� �������� ������������ ���������� ������ �������� � email
// ����� �� ���������, ����� � input-a ��������� �������� �������� class � "email" ��� "phone"
// � ��� �������� ��������� ������� ����� validate � �����
jQuery(function() {
	jQuery.validator.addMethod("phone", function(value, element) {
            if (value.toString().length > 0) {
                res = /^(\d{3,4})-(\d{6,7})(\*\d{3})?$/.test(value)
                return res;
            }
            return true;
        }, "������������ ���������� �����");
		
	// ��������� ��� ����������� ������
	jQuery.validator.addMethod("InternalPhone", function(value, element) {
            if (value.toString().length > 0) {
                res = /^(\d{3})$/.test(value)
                return res;
            }
            return true;
        }, "������������ ���������� �����");		

        jQuery.validator.addMethod("email", function(value, element) {
            if (value.toString().length > 0) {
                res = /^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/.test(value)
                return res;
            }
            return true;
        }, "������������ ����� ����������� �����");  
});