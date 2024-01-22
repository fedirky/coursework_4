import socket


def send_search_request(server_ip, server_port, search_term):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
        sock.connect((server_ip, server_port))
        sock.sendall(search_term.encode('utf-8'))
        response = sock.recv(12288)
        return response.decode('utf-8')


def main():
    server_ip = "127.0.0.1"  # Адреса сервера
    server_port = 8080  # Порт сервера

    while True:
        search_term = input("Введіть слово для пошуку: ")
        if not search_term:
            break

        result = send_search_request(server_ip, server_port, search_term)
        if result:
            print("Результати пошуку:")
            print(result)  # Виводимо результат як строку
        else:
            print("Слово не знайдено або виникла помилка.")


if __name__ == "__main__":
    main()

