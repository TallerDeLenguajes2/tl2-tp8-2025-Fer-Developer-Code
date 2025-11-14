using Microsoft.AspNetCore.Mvc;
using Interfaces;
using ViewModels;

namespace Controllers;

public class LoginController : Controller
{
    // --- INYECCIÓN DE DEPENDENCIAS ---
    private readonly IAuthenticationService _authService;

    public LoginController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    // --- MUESTRA LA PÁGINA DE LOGIN ---
    // GET: /Login/Index o /Login
    [HttpGet]
    public IActionResult Index()
    {
        // Devuelve un ViewModel vacío para el formulario
        return View(new LoginViewModel());
    }

    // --- PROCESA EL INTENTO DE LOGIN ---
    // POST: /Login/Index o /Login
    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        // Verificamos las validaciones (Required)
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        // Usamos nuestro servicio para intentar el login
        if (_authService.Login(model.Username, model.Password))
        {
            // ¡Éxito! Redirigimos al inicio de la aplicación
            return RedirectToAction("Index", "Home");
        }
        else
        {
            // Fracaso: Añadimos un error y volvemos a mostrar el form
            model.ErrorMessage = "Credenciales inválidas.";
            return View("Index", model);
        }
    }

    // --- CIERRA LA SESIÓN ---
    // GET: /Login/Logout
    [HttpGet]
    public IActionResult Logout()
    {
        _authService.Logout(); // Llama al servicio para limpiar la sesión
        return RedirectToAction("Index", "Login"); // Manda al usuario al login
    }
}