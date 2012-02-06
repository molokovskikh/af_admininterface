#include "Printer.h"
#include <iostream>
#include <QPrinterInfo.h>

Printer::Printer(QObject *parent) :
    QObject(parent)
{
	m_debug = false;
}

void Printer::load(QUrl url)
{
    connect(&m_page, SIGNAL(loadProgress(int)), this, SLOT(printProgress(int)));
    connect(&m_page, SIGNAL(loadFinished(bool)), this, SLOT(saveResult(bool)));
    m_page.mainFrame()->load(url);
}

void Printer::printProgress(int percent)
{
	if (m_debug)
	{
		std::cout << "#" << std::flush;
	}
}

void Printer::print()
{
	if (m_debug)
	{
		std::cout << "paint" << std::endl;
		std::cout << qPrintable(m_page.mainFrame()->toHtml()) << std::endl;
	}

    QList<QPrinterInfo> printers = QPrinterInfo::availablePrinters();
    int printerIndex = -1;
    for(int i = 0; i < printers.length(); i++)
    {
        if (QString::compare(m_printer, printers[i].printerName(), Qt::CaseInsensitive) == 0)
        {
            printerIndex = i;
            break;
        }
    }
    if (printerIndex > -1)
    {
        QPrinter printer(printers[printerIndex]);
        QRectF rect = printer.pageRect(QPrinter::DevicePixel);
        m_page.setViewportSize(rect.toAlignedRect().size());
        QPainter *painter = new QPainter(&printer);
        m_page.mainFrame()->documentElement().render(painter);
        painter->end();
    }
}

//для отладки
void Printer::render(QWebFrame *frame)
{
    QImage image(frame->contentsSize(), QImage::Format_ARGB32_Premultiplied);
    image.fill(Qt::transparent);

    QPainter painter(&image);
    painter.setRenderHint(QPainter::Antialiasing, true);
    painter.setRenderHint(QPainter::TextAntialiasing, true);
    painter.setRenderHint(QPainter::SmoothPixmapTransform, true);
    m_page.mainFrame()->documentElement().render(&painter);
    painter.end();

    image.save("C:\\test.png");
}

void Printer::setPrinter(QString name)
{
    m_printer = name;
}

void Printer::saveResult(bool ok)
{
	if (m_debug)
	{
		std::cout << "saveResult" << std::endl;
	}

    // crude error-checking
    if (!ok) {
		QString r = m_page.mainFrame()->url().toString();
        std::cerr << "Failed loading " << qPrintable(m_page.mainFrame()->url().toString()) << std::endl;
    }
    else
    {
        m_page.setViewportSize(m_page.mainFrame()->contentsSize());
        print();
    }
	if (m_debug)
	{
		std::cout << "finished" << std::endl;
	}
    emit finished();
}
