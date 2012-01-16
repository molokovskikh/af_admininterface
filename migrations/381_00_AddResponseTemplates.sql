update UserSettings.Defaults 
set
  ResponseBodyMiniMailOnUnknownProvider = 'Предполагаемая дата запуска функционала 20.01.12.',
  ResponseBodyMiniMailOnEmptyRecipients = 'Предполагаемая дата запуска функционала 20.01.12.',
  ResponseBodyMiniMailOnMaxAttachment = 'Предполагаемая дата запуска функционала 20.01.12.',
  ResponseBodyMiniMailOnAllowedExtensions = 'Предполагаемая дата запуска функционала 20.01.12.'
where
  Id = 1;
