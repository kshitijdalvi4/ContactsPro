﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContactsPro.Data;
using ContactsPro.Models;
using ContactsPro.Enums;
using ContactsPro.Services;
using ContactsPro.Services.Interfaces;

namespace ContactsPro.Controllers
{ 

public class ContactsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IImageService _imageService;
    private readonly IAddressBookService _addressBookService;
    public ContactsController(ApplicationDbContext context, UserManager<AppUser> userManager,IImageService imageService,IAddressBookService addressBookService)
    {
        _context = context;
        _userManager = userManager;
        _imageService = imageService;
        _addressBookService = addressBookService;
    }

    // GET: Contacts
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var applicationDbContext = _context.Contacts.Include(c => c.AppUser);
        return View(await applicationDbContext.ToListAsync());
    }

    // GET: Contacts/Details/5
    [Authorize]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null || _context.Contacts == null)
        {
            return NotFound();
        }

        var contact = await _context.Contacts
            .Include(c => c.AppUser)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (contact == null)
        {
            return NotFound();
        }

        return View(contact);
    }

    // GET: Contacts/Create
    [Authorize]
    public async Task<IActionResult> Create()
    {
        string appUserId = _userManager.GetUserId(User);

        ViewData["StatesList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
        ViewData["CategoryList"]= new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId),"Id","Name");
        return View();
    }

    // POST: Contacts/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,ImageFile")] Contact contact,List<int> CategoryList)
    {

        ModelState.Remove("AppUserId");
        if (ModelState.IsValid)
        {
            contact.AppUserID = _userManager.GetUserId(User);
            contact.Created= DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);


            if( contact.BirthDate !=null)
            {
                contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);

            }

                if (contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }

            _context.Add(contact);
            await _context.SaveChangesAsync();

                //loo[ over all selected categories

                foreach (int categoryId in CategoryList)
                {
                    await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                }
                //Svae each cat to contactcategoriess table
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Contacts/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null || _context.Contacts == null)
        {
            return NotFound();
        }

        var contact = await _context.Contacts.FindAsync(id);
        if (contact == null)
        {
            return NotFound();
        }
        ViewData["AppUserID"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserID);
        return View(contact);
    }

    // POST: Contacts/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserID,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageData,ImageType")] Contact contact)
    {
        if (id != contact.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(contact);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactExists(contact.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["AppUserID"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserID);
        return View(contact);
    }

    // GET: Contacts/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null || _context.Contacts == null)
        {
            return NotFound();
        }
            string appUserId = _userManager.GetUserId(User);

            var contact = await _context.Contacts
            .Include(c => c.AppUser)
            .FirstOrDefaultAsync(m => m.Id == id && m.AppUserId == appUserId);
        if (contact == null)
        {
            return NotFound();
        }

        return View(contact);
    }

    // POST: Contacts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Contacts == null)
        {
            return Problem("Entity set 'ApplicationDbContext.Contacts'  is null.");
        }
        var contact = await _context.Contacts.FindAsync(id);
        if (contact != null)
        {
            _context.Contacts.Remove(contact);
        }
        
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ContactExists(int id)
    {
      return (_context.Contacts?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}
}