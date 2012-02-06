#ifndef TEST_H
#define TEST_H

#include <QObject>
#include <QtWebKit>

class Printer : public QObject
{
    Q_OBJECT
public:
    explicit Printer(QObject *parent = 0);
    void load(QUrl url);
    void setPrinter(QString name);
    void print();
    void render(QWebFrame *frame);
signals:
    void finished();
public slots:
    void printProgress(int percent);
    void saveResult(bool ok);
private:
    QWebPage m_page;
    QString m_printer;
	bool m_debug;
};

#endif // TEST_H
