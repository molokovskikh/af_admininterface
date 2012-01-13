alter table UserSettings.Defaults 
  add column AllowedMiniMailExtensions VARCHAR(255) not null default 'doc, xls, gif, tiff, tif, jpg, pdf, txt';
