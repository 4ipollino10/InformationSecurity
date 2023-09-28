namespace CryptoMethods.CryptoMethods;

/// <summary>
/// Класс, использующий шифрование, используя линейный конгруентный генератор 
/// </summary>
public class LinearCongruentGeneratorCipher
{
    /// <summary>
    /// 8-е число Марсена (число m)
    /// </summary>
    private const long RangeOfValues = 2147483647;

    /// <summary>
    /// Множитель а
    /// </summary>S
    private const long Factor = 2147483648;

    /// <summary>
    /// Приращение с
    /// </summary>
    private const long Increment = 73;

    /// <summary>
    /// Стартовое значение Xo
    /// </summary>
    private const long StartValue = 1;

    /// <summary>
    /// Текущее псевдослучайное число
    /// </summary>
    private long _currentNumber = StartValue;
    
    /// <summary>
    /// Получение следующего случайного числа
    /// по формуле Xi+1 = (aXi + c) % m
    /// </summary>
    private void NextNumber()
    {
        _currentNumber = (Factor * _currentNumber + Increment) % RangeOfValues;
    }

    /// <summary>
    /// Таблица трансляций
    /// </summary>
    private Dictionary<string, char> _translationTable;

    /// <summary>
    /// Зашифрованная строка
    /// </summary>
    private string? _encryptedString; 
    
    /// <summary>
    /// конструктор с инициализацией словаря
    /// </summary>
    public LinearCongruentGeneratorCipher()
    {
        _translationTable = new Dictionary<string, char>();
    }
    
    /// <summary>
    /// Шифрование входной строки
    /// Шифрование происходит путем сопоставления каждого символа строки со сгенерированным числом
    /// параметры подобраны так, чтобы длина периода была равна <see cref="RangeOfValues"/>, что позволит однозначно дешифровать строки
    /// </summary>
    /// <param name="encryptionString"></param>
    public string EncryptString(string encryptionString)
    {
        //при шифровании новой строки обновляем словарь и строку
        _translationTable = new Dictionary<string, char>();
        var encryptedString = string.Empty;
        
        foreach (var character in encryptionString)
        {
            _translationTable.Add(_currentNumber.ToString(), character);
            encryptedString += _currentNumber.ToString(); 
            NextNumber();
        }

        _encryptedString = encryptedString;
        
        return encryptedString;
    }

    /// <summary>
    /// Дешифрование зашифрованной строки
    /// при дешифровании происходит парсинг зашифрованной строки и получени соответсвующего символа из словаря, после происходит удаление пары, чтобы в нее больше не попасть
    /// однозначность достигается параметрами подобранными по науке (я почитал википедию)
    /// </summary>
    public string? DecryptString()
    {
        if (_encryptedString == null)
        {
            return null;
        }

        var decryptedString = string.Empty;
        var key = string.Empty;
        foreach (var num in _encryptedString)
        {
            key += num;
            if (_translationTable.TryGetValue(key, out var value))
            {
                decryptedString += value;
            }
            else
            {
                continue;
            }

            _translationTable.Remove(key);
            key = string.Empty;
        }

        return decryptedString;
    }


}