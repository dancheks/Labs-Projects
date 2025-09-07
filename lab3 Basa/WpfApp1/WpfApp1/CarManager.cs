using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class CarManager
{
    private List<Car> cars = new List<Car>();

    public void AddCar(Car car)
    {
        cars.Add(car);
    }

    public void RemoveCar(Car car)
    {
        cars.Remove(car);
    }

    public List<Car> SearchCars(string brand, string model)
    {
        return cars.FindAll(c => c.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase) &&
                                  c.Model.Equals(model, StringComparison.OrdinalIgnoreCase));
    }

    public List<Car> GetAllCars()
    {
        return cars;
    }

    public void SaveToBinary(string filePath)
    {
        IFormatter formatter = new BinaryFormatter();
        using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            formatter.Serialize(stream, cars);
        }
    }

    public void SaveToXml(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<Car>));
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, cars);
        }
    }

    public List<Car> LoadFromBinary(string filePath)
    {
        IFormatter formatter = new BinaryFormatter();
        using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            return (List<Car>)formatter.Deserialize(stream);
        }
    }

    public List<Car> LoadFromXml(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<Car>));
        using (StreamReader reader = new StreamReader(filePath))
        {
            return (List<Car>)serializer.Deserialize(reader);
        }
    }
}