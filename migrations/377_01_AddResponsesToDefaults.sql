alter table UserSettings.Defaults 
  add column ResponseSubjectMiniMailOnUnknownProvider VARCHAR(255) not null default 'Письмо не доставлено аптеке',
  add column ResponseBodyMiniMailOnUnknownProvider mediumtext,
  add column ResponseSubjectMiniMailOnEmptyRecipients VARCHAR(255) not null default 'Письмо не доставлено аптеке',
  add column ResponseBodyMiniMailOnEmptyRecipients mediumtext,
  add column ResponseSubjectMiniMailOnMaxAttachment VARCHAR(255) not null default 'Письмо не доставлено аптеке',
  add column ResponseBodyMiniMailOnMaxAttachment mediumtext,
  add column ResponseSubjectMiniMailOnAllowedExtensions VARCHAR(255) not null default 'Письмо не доставлено аптеке',
  add column ResponseBodyMiniMailOnAllowedExtensions mediumtext;
