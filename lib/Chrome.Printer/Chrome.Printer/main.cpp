#include <QtGui/QApplication>
#include <QtCore/QCoreApplication>
#include <QWebPage>
#include <QWebFrame>
#include <QTHread>
#include <QPainter>
#include <QPrinterInfo.h>
#include "Printer.h"

int main(int argc, char *argv[])
{
    QApplication app(argc, argv);
    Printer printer;
	QStringList args = QCoreApplication::arguments();
    printer.setPrinter(args[1]);
	QString s = args[2];
    printer.load(args[2]);
    QObject::connect(&printer, SIGNAL(finished()), &app, SLOT(quit()));
    return app.exec();
}
