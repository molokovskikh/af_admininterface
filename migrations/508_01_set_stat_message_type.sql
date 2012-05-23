update `logs`.clientsinfo c
set MessageType = 2
where Message like '$$$Изменено \'Комментарий\'%'
and ishtml = 1;
